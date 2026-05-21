using FFS.Libraries.StaticEcs;
using StaticMlp.Game;
using StaticMlp.Networking;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class RuntimeNavMeshRebuildQueueSystem : ISystem
    {
        public void Update()
        {
            ResetCounters();

            var simulationTime = SW.GetResource<SimulationTime>();
            var registry = SW.GetResource<ChunkNavSourceRegistry>();
            foreach (var entity in SW.Query<All<NavInterestArea, RuntimeNavMeshZoneState, CombatCellPerformanceBudget, NavWorkBudgetCounter>>().Entities())
                QueueRebuildIfNeeded(entity, simulationTime.ServerTick, registry);
        }

        private static void ResetCounters()
        {
            foreach (var entity in SW.Query<All<NavWorkBudgetCounter>>().Entities())
                entity.Mut<NavWorkBudgetCounter>() = default;
        }

        private static void QueueRebuildIfNeeded(SW.Entity entity, uint currentTick, ChunkNavSourceRegistry registry)
        {
            ref readonly var navArea = ref entity.Read<NavInterestArea>();
            ref var zoneState = ref entity.Mut<RuntimeNavMeshZoneState>();
            ref var counters = ref entity.Mut<NavWorkBudgetCounter>();
            var queuedCenter = NavInterestAreaRules.QuantizeNavCenter(navArea.Center);
            var sourceCollectBounds = RuntimeNavMeshZoneBounds.Create(queuedCenter, navArea.SourceCollectRadius);
            var sourceSetVersion = registry.CalculateSourceSetVersion(sourceCollectBounds, out _);

            if (!NeedsRebuild(in navArea, in zoneState, queuedCenter, sourceSetVersion))
                return;

            var hasPendingRequest = entity.Has<NavRebuildRequest>();
            var isBuilding = zoneState.BuildState == RuntimeNavMeshBuildState.Building;
            var reason = zoneState.RequestedNavVersion == 0
                ? NavRebuildReason.InitialBuild
                : sourceSetVersion != zoneState.RequestedSourceSetVersion
                    ? NavRebuildReason.SourceGeometryChanged
                    : NavRebuildReason.NavAreaChanged;

            if (!hasPendingRequest || isBuilding)
                zoneState.RequestedNavVersion++;

            zoneState.RequestedSourceSetVersion = sourceSetVersion;
            zoneState.LastObservedRegistryVersion = registry.RegistryVersion;
            if (!isBuilding)
                zoneState.BuildState = RuntimeNavMeshBuildState.Queued;
            zoneState.LastQueuedCenter = queuedCenter;
            zoneState.LastQueuedRadius = navArea.Radius;
            zoneState.LastQueuedNavBuildRadius = navArea.NavBuildRadius;
            zoneState.LastQueuedSourceCollectRadius = navArea.SourceCollectRadius;
            zoneState.LastQueuedPriority = navArea.Priority;

            var request = new NavRebuildRequest
            {
                Reason = reason,
                Priority = navArea.Priority,
                RequestedAtTick = currentTick,
                AllowDuringPeak = false
            };

            if (hasPendingRequest)
            {
                ref var current = ref entity.Mut<NavRebuildRequest>();
                if (request.Priority > current.Priority)
                    current.Priority = request.Priority;

                if (request.RequestedAtTick < current.RequestedAtTick)
                    current.RequestedAtTick = request.RequestedAtTick;

                current.Reason = request.Reason;
                current.AllowDuringPeak |= request.AllowDuringPeak;
            }
            else
            {
                entity.Set(request);
            }

            counters.RebuildRequestsQueuedThisTick++;
        }

        private static bool NeedsRebuild(
            in NavInterestArea navArea,
            in RuntimeNavMeshZoneState zoneState,
            float3 queuedCenter,
            ulong sourceSetVersion)
        {
            if (sourceSetVersion == 0ul
                && zoneState.SourceSetVersion == 0ul
                && zoneState.RequestedSourceSetVersion == 0ul)
                return false;

            return zoneState.RequestedNavVersion == 0
                   || sourceSetVersion != zoneState.RequestedSourceSetVersion
                   || navArea.Priority != zoneState.LastQueuedPriority
                   || navArea.Radius != zoneState.LastQueuedRadius
                   || navArea.NavBuildRadius != zoneState.LastQueuedNavBuildRadius
                   || navArea.SourceCollectRadius != zoneState.LastQueuedSourceCollectRadius
                   || queuedCenter.x != zoneState.LastQueuedCenter.x
                   || queuedCenter.y != zoneState.LastQueuedCenter.y
                   || queuedCenter.z != zoneState.LastQueuedCenter.z;
        }
    }
}
