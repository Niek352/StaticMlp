using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Frontier;
using StaticMlp.Game;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class RuntimeNavMeshBuildSystem : ISystem
    {
        private const float REBUILD_DEBOUNCE_SECONDS = 0.5f;

        private readonly List<RuntimeNavMeshZoneBackend.BuildResult> _completedBuilds = new();

        public void Update()
        {
            var backend = SW.GetResource<RuntimeNavMeshZoneBackend>();
            CompleteFinishedBuilds(backend);

            var pending = TrySelectPendingRequest(out var phase, out var requestEntity);
            if (!pending)
                return;

            ref readonly var request = ref requestEntity.Read<NavRebuildRequest>();
            ref readonly var budget = ref requestEntity.Read<CombatCellPerformanceBudget>();
            ref var zoneState = ref requestEntity.Mut<RuntimeNavMeshZoneState>();
            ref var counters = ref requestEntity.Mut<NavWorkBudgetCounter>();
            var simulationTime = SW.GetResource<SimulationTime>();

            if (counters.RebuildsStartedThisTick >= budget.MaxNavRebuildStartsPerTick)
                return;

            if (simulationTime.ServerTick < zoneState.NextAllowedRebuildTick)
                return;

            if (phase == FrontierNavWorkPhase.Peak && !request.AllowDuringPeak)
                return;

            if (backend.IsBuilding(requestEntity.GID))
                return;

            var registry = SW.GetResource<ChunkNavSourceRegistry>();
            var sourceCollectBounds = RuntimeNavMeshZoneBounds.Create(zoneState.LastQueuedCenter, zoneState.LastQueuedSourceCollectRadius);
            var sourceSetVersion = registry.CalculateSourceSetVersion(sourceCollectBounds, out _);
            if (sourceSetVersion == 0ul)
            {
                backend.Remove(requestEntity.GID);
                zoneState.NavVersion = zoneState.RequestedNavVersion;
                zoneState.SourceSetVersion = 0ul;
                zoneState.RequestedSourceSetVersion = 0ul;
                zoneState.BuildState = RuntimeNavMeshBuildState.None;
                requestEntity.Delete<NavRebuildRequest>();
                return;
            }

            if (sourceSetVersion != zoneState.RequestedSourceSetVersion)
            {
                zoneState.RequestedSourceSetVersion = sourceSetVersion;
                zoneState.RequestedNavVersion++;
            }

            zoneState.BuildState = RuntimeNavMeshBuildState.Building;
            zoneState.NextAllowedRebuildTick = simulationTime.ServerTick + ComputeRebuildCooldownTicks(in simulationTime);
            counters.RebuildsStartedThisTick++;

            backend.StartBuild(
                requestEntity.GID,
                new RuntimeNavMeshZoneBackend.BuildInput(
                    RuntimeNavMeshZoneBounds.Create(zoneState.LastQueuedCenter, zoneState.LastQueuedNavBuildRadius),
                    sourceCollectBounds,
                    zoneState.RequestedNavVersion,
                    zoneState.RequestedSourceSetVersion),
                registry);
        }

        private void CompleteFinishedBuilds(RuntimeNavMeshZoneBackend backend)
        {
            _completedBuilds.Clear();
            backend.CollectCompletedBuilds(_completedBuilds);

            for (var i = 0; i < _completedBuilds.Count; i++)
            {
                var result = _completedBuilds[i];
                if (!result.ZoneId.TryUnpack<ServerWT>(out var entity))
                {
                    backend.Remove(result.ZoneId);
                    continue;
                }

                if (!entity.Has<RuntimeNavMeshZoneState>())
                {
                    backend.Remove(result.ZoneId);
                    continue;
                }

                ref var zoneState = ref entity.Mut<RuntimeNavMeshZoneState>();
                zoneState.NavVersion = result.NavVersion;
                zoneState.SourceSetVersion = result.SourceSetVersion;

                if (entity.Has<NavRebuildRequest>()
                    && (zoneState.RequestedNavVersion != result.NavVersion
                        || zoneState.RequestedSourceSetVersion != result.SourceSetVersion))
                {
                    zoneState.BuildState = RuntimeNavMeshBuildState.Queued;
                    continue;
                }

                zoneState.BuildState = RuntimeNavMeshBuildState.Ready;
                if (entity.Has<NavRebuildRequest>())
                    entity.Delete<NavRebuildRequest>();
            }

            _completedBuilds.Clear();
        }

        private static uint ComputeRebuildCooldownTicks(in SimulationTime simulationTime)
        {
            var ticks = simulationTime.SecondsToTicks(REBUILD_DEBOUNCE_SECONDS);
            return ticks == 0 ? 1u : ticks;
        }

        private static bool TrySelectPendingRequest(out FrontierNavWorkPhase phase, out SW.Entity selectedEntity)
        {
            phase = FrontierNavWorkPhase.Calm;
            selectedEntity = default;

            var hasRequests = false;
            var found = false;
            var bestPriority = int.MinValue;
            uint bestTick = uint.MaxValue;

            foreach (var entity in SW.Query<All<NavInterestArea, RuntimeNavMeshZoneState, NavRebuildRequest, CombatCellPerformanceBudget, NavWorkBudgetCounter>>().Entities())
            {
                hasRequests = true;
            }

            if (!hasRequests)
                return false;

            phase = ResolvePhase();

            foreach (var entity in SW.Query<All<NavInterestArea, RuntimeNavMeshZoneState, NavRebuildRequest, CombatCellPerformanceBudget, NavWorkBudgetCounter>>().Entities())
            {
                ref readonly var request = ref entity.Read<NavRebuildRequest>();
                ref readonly var zoneState = ref entity.Read<RuntimeNavMeshZoneState>();

                if (zoneState.BuildState == RuntimeNavMeshBuildState.Building)
                    continue;

                if (phase == FrontierNavWorkPhase.Peak && !request.AllowDuringPeak)
                    continue;

                if (!found || request.Priority > bestPriority || request.Priority == bestPriority && request.RequestedAtTick < bestTick)
                {
                    selectedEntity = entity;
                    bestPriority = request.Priority;
                    bestTick = request.RequestedAtTick;
                    found = true;
                }
            }

            return found;
        }

        private static FrontierNavWorkPhase ResolvePhase()
        {
            var found = false;
            var phase = FrontierNavWorkPhase.Calm;

            foreach (var entity in SW.Query<All<FrontierNavWorkPhaseState>>().Entities())
            {
                var current = entity.Read<FrontierNavWorkPhaseState>().Phase;
                if (!found || current > phase)
                {
                    phase = current;
                    found = true;
                }
            }

            if (!found)
                throw new InvalidOperationException("FrontierNavWorkPhaseState is required for runtime nav rebuild throttling.");

            return phase;
        }
    }
}
