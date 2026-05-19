using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Frontier;
using StaticMlp.Game;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class RuntimeNavMeshBuildSystem : ISystem
    {
        public void Update()
        {
            var pending = TrySelectPendingRequest(out var phase, out var requestEntity);
            if (!pending)
                return;

            ref readonly var request = ref requestEntity.Read<NavRebuildRequest>();
            ref readonly var budget = ref requestEntity.Read<CombatCellPerformanceBudget>();
            ref readonly var navArea = ref requestEntity.Read<CombatCellNavArea>();
            ref var zoneState = ref requestEntity.Mut<RuntimeNavMeshZoneState>();
            ref var counters = ref requestEntity.Mut<NavWorkBudgetCounter>();

            if (counters.RebuildsStartedThisTick >= budget.MaxNavRebuildStartsPerTick)
                return;

            if (SW.GetResource<SimulationTime>().ServerTick < zoneState.NextAllowedRebuildTick)
                return;

            if (phase == FrontierNavWorkPhase.Peak && !request.AllowDuringPeak)
                return;

            zoneState.BuildState = RuntimeNavMeshBuildState.Building;
            zoneState.NextAllowedRebuildTick = SW.GetResource<SimulationTime>().ServerTick + 1;
            counters.RebuildsStartedThisTick++;

            throw new InvalidOperationException(
                $"Runtime NavMesh backend is not implemented for combat cell {navArea.CellId}. " +
                $"Queued nav rebuild version {zoneState.RequestedNavVersion} cannot be executed.");
        }

        private static bool TrySelectPendingRequest(out FrontierNavWorkPhase phase, out SW.Entity selectedEntity)
        {
            phase = FrontierNavWorkPhase.Calm;
            selectedEntity = default;

            var hasRequests = false;
            var found = false;
            var bestPriority = int.MinValue;
            uint bestTick = uint.MaxValue;

            foreach (var entity in SW.Query<All<CombatCellNavArea, RuntimeNavMeshZoneState, NavRebuildRequest, CombatCellPerformanceBudget, NavWorkBudgetCounter>>().Entities())
            {
                hasRequests = true;
            }

            if (!hasRequests)
                return false;

            phase = ResolvePhase();

            foreach (var entity in SW.Query<All<CombatCellNavArea, RuntimeNavMeshZoneState, NavRebuildRequest, CombatCellPerformanceBudget, NavWorkBudgetCounter>>().Entities())
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
