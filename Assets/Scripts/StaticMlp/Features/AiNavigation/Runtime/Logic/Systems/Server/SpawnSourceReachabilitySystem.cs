using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Frontier;
using StaticMlp.Game;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class SpawnSourceReachabilitySystem : ISystem
    {
        public void Update()
        {
            var currentTime = (float)SW.GetResource<SimulationTime>().ElapsedSeconds;

            foreach (var entity in SW.Query<All<SpawnSource>>().Entities())
                ScheduleReachability(entity, currentTime);

            foreach (var navAreaEntity in SW.Query<All<CombatCellNavArea, RuntimeNavMeshZoneState, CombatCellPerformanceBudget, NavWorkBudgetCounter>>().Entities())
                ProcessReachability(navAreaEntity, currentTime);

            CleanupStaleRequests();
        }

        private static void ScheduleReachability(SW.Entity sourceEntity, float currentTime)
        {
            ref readonly var spawnSource = ref sourceEntity.Read<SpawnSource>();
            ref var navState = ref EnsureNavState(sourceEntity);

            if (!spawnSource.IsActive)
            {
                ClearRequest(sourceEntity);
                if (SpawnSourceReachabilityRules.NeedsReachabilityCheck(in navState, 0, currentTime))
                    navState = SpawnSourceReachabilityRules.CreateOutOfAreaState(currentTime);

                return;
            }

            if (!TryResolveNavArea(spawnSource.Position, out var navAreaEntity))
            {
                ClearRequest(sourceEntity);
                if (SpawnSourceReachabilityRules.NeedsReachabilityCheck(in navState, 0, currentTime))
                    navState = SpawnSourceReachabilityRules.CreateOutOfAreaState(currentTime);

                return;
            }

            ref readonly var navArea = ref navAreaEntity.Read<CombatCellNavArea>();
            ref readonly var zoneState = ref navAreaEntity.Read<RuntimeNavMeshZoneState>();
            var approxPathCost = math.distance(navArea.Center, spawnSource.Position);

            if (!SpawnSourceReachabilityRules.NeedsReachabilityCheck(in navState, zoneState.NavVersion, currentTime))
            {
                ClearRequest(sourceEntity);
                return;
            }

            var request = SpawnSourceReachabilityRules.CreateRequest(navArea.CellId, zoneState.NavVersion, currentTime, approxPathCost);
            if (sourceEntity.Has<SpawnSourceReachabilityRequest>())
            {
                ref var currentRequest = ref sourceEntity.Mut<SpawnSourceReachabilityRequest>();
                currentRequest = request;
            }
            else
            {
                sourceEntity.Set(request);
            }

            navState = SpawnSourceReachabilityRules.CreateDeferredState(in navState, in zoneState, currentTime, approxPathCost);
        }

        private static void ProcessReachability(SW.Entity navAreaEntity, float currentTime)
        {
            ref readonly var navArea = ref navAreaEntity.Read<CombatCellNavArea>();
            ref readonly var zoneState = ref navAreaEntity.Read<RuntimeNavMeshZoneState>();
            ref readonly var budget = ref navAreaEntity.Read<CombatCellPerformanceBudget>();
            ref var counters = ref navAreaEntity.Mut<NavWorkBudgetCounter>();

            var remainingBudget = budget.MaxReachabilityChecksPerTick - counters.ReachabilityChecksThisTick;
            while (remainingBudget > 0 && TrySelectRequest(navArea.CellId, out var requestEntity))
            {
                var request = requestEntity.Read<SpawnSourceReachabilityRequest>();
                var status = zoneState.BuildState == RuntimeNavMeshBuildState.Ready
                    ? SpawnSourceNavStatus.Reachable
                    : SpawnSourceNavStatus.WaitingForNavMesh;

                requestEntity.Mut<SpawnSourceNavState>() = SpawnSourceReachabilityRules.CreateCheckedState(
                    status,
                    request.NavVersion,
                    currentTime,
                    request.ApproxPathCost);

                requestEntity.Delete<SpawnSourceReachabilityRequest>();
                counters.ReachabilityChecksThisTick++;
                remainingBudget--;
            }
        }

        private static void CleanupStaleRequests()
        {
            foreach (var entity in SW.Query<All<SpawnSourceReachabilityRequest>, None<SpawnSource>>().Entities())
                entity.Delete<SpawnSourceReachabilityRequest>();
        }

        private static ref SpawnSourceNavState EnsureNavState(SW.Entity sourceEntity)
        {
            if (!sourceEntity.Has<SpawnSourceNavState>())
                sourceEntity.Set(SpawnSourceReachabilityRules.CreateInitialState());

            return ref sourceEntity.Mut<SpawnSourceNavState>();
        }

        private static void ClearRequest(SW.Entity sourceEntity)
        {
            if (sourceEntity.Has<SpawnSourceReachabilityRequest>())
                sourceEntity.Delete<SpawnSourceReachabilityRequest>();
        }

        private static bool TryResolveNavArea(float3 sourcePosition, out SW.Entity bestEntity)
        {
            bestEntity = default;

            var found = false;
            var bestPriority = int.MinValue;
            var bestDistanceSq = float.MaxValue;

            foreach (var entity in SW.Query<All<CombatCellNavArea, RuntimeNavMeshZoneState, CombatCellPerformanceBudget, NavWorkBudgetCounter>>().Entities())
            {
                ref readonly var navArea = ref entity.Read<CombatCellNavArea>();
                if (!SpawnSourceReachabilityRules.ContainsSource(in navArea, sourcePosition))
                    continue;

                var distanceSq = math.distancesq(navArea.Center, sourcePosition);
                if (!SpawnSourceReachabilityRules.IsBetterAreaCandidate(found, navArea.Priority, distanceSq, bestPriority, bestDistanceSq))
                    continue;

                bestEntity = entity;
                bestPriority = navArea.Priority;
                bestDistanceSq = distanceSq;
                found = true;
            }

            return found;
        }

        private static bool TrySelectRequest(int cellId, out SW.Entity selectedEntity)
        {
            selectedEntity = default;

            var found = false;
            var bestRequestedAtTime = float.MaxValue;
            var bestApproxPathCost = float.MaxValue;

            foreach (var entity in SW.Query<All<SpawnSource, SpawnSourceReachabilityRequest, SpawnSourceNavState>>().Entities())
            {
                ref readonly var request = ref entity.Read<SpawnSourceReachabilityRequest>();
                if (request.CellId != cellId)
                    continue;

                if (!found
                    || request.RequestedAtTime < bestRequestedAtTime
                    || request.RequestedAtTime == bestRequestedAtTime && request.ApproxPathCost < bestApproxPathCost)
                {
                    selectedEntity = entity;
                    bestRequestedAtTime = request.RequestedAtTime;
                    bestApproxPathCost = request.ApproxPathCost;
                    found = true;
                }
            }

            return found;
        }
    }
}
