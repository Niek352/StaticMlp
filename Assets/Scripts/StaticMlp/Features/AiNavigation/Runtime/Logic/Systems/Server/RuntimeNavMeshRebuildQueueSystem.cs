using FFS.Libraries.StaticEcs;
using StaticMlp.Game;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class RuntimeNavMeshRebuildQueueSystem : ISystem
    {
        public void Update()
        {
            ResetCounters();

            var simulationTime = SW.GetResource<SimulationTime>();
            foreach (var entity in SW.Query<All<CombatCellNavArea, RuntimeNavMeshZoneState, CombatCellPerformanceBudget, NavWorkBudgetCounter>>().Entities())
                QueueRebuildIfNeeded(entity, simulationTime.ServerTick);
        }

        private static void ResetCounters()
        {
            foreach (var entity in SW.Query<All<NavWorkBudgetCounter>>().Entities())
                entity.Mut<NavWorkBudgetCounter>() = default;
        }

        private static void QueueRebuildIfNeeded(SW.Entity entity, uint currentTick)
        {
            ref readonly var navArea = ref entity.Read<CombatCellNavArea>();
            ref var zoneState = ref entity.Mut<RuntimeNavMeshZoneState>();
            ref var counters = ref entity.Mut<NavWorkBudgetCounter>();

            if (!NeedsRebuild(in navArea, in zoneState))
                return;

            var reason = zoneState.RequestedNavVersion == 0
                ? NavRebuildReason.InitialBuild
                : NavRebuildReason.CombatCellNavAreaChanged;

            zoneState.RequestedNavVersion++;
            zoneState.BuildState = RuntimeNavMeshBuildState.Queued;
            zoneState.LastQueuedCenter = navArea.Center;
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

            if (entity.Has<NavRebuildRequest>())
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

        private static bool NeedsRebuild(in CombatCellNavArea navArea, in RuntimeNavMeshZoneState zoneState)
        {
            return zoneState.RequestedNavVersion == 0
                   || navArea.Priority != zoneState.LastQueuedPriority
                   || navArea.Radius != zoneState.LastQueuedRadius
                   || navArea.NavBuildRadius != zoneState.LastQueuedNavBuildRadius
                   || navArea.SourceCollectRadius != zoneState.LastQueuedSourceCollectRadius
                   || navArea.Center.x != zoneState.LastQueuedCenter.x
                   || navArea.Center.y != zoneState.LastQueuedCenter.y
                   || navArea.Center.z != zoneState.LastQueuedCenter.z;
        }
    }
}
