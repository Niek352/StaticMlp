using FFS.Libraries.StaticEcs;
using StaticMlp.Features.CombatDirector;
using StaticMlp.Networking;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class ReachableSpawnSourceCacheSystem : ISystem
    {
        public void Update()
        {
            foreach (var sourceEntity in SW.Query<All<SpawnSource, SpawnSourceNavState>>().Entities())
                UpdateCandidate(sourceEntity);

            CleanupDeletedSources();
        }

        private static void UpdateCandidate(SW.Entity sourceEntity)
        {
            ref readonly var spawnSource = ref sourceEntity.Read<SpawnSource>();
            ref readonly var navState = ref sourceEntity.Read<SpawnSourceNavState>();

            if (!spawnSource.IsActive
                || navState.Status != SpawnSourceNavStatus.Reachable
                || !TryResolveNavArea(spawnSource.Position, out var navAreaEntity))
            {
                DeleteCandidate(sourceEntity);
                return;
            }

            ref readonly var navArea = ref navAreaEntity.Read<NavInterestArea>();
            ref readonly var zoneState = ref navAreaEntity.Read<RuntimeNavMeshZoneState>();

            if (zoneState.NavVersion != navState.NavVersion)
            {
                DeleteCandidate(sourceEntity);
                return;
            }

            var candidate = new ReachableSpawnSourceCandidate
            {
                Source = sourceEntity.GID,
                ZoneId = navArea.AreaId,
                NavVersion = navState.NavVersion,
                ApproxPathCost = navState.ApproxPathCost
            };

            if (sourceEntity.Has<ReachableSpawnSourceCandidate>())
            {
                sourceEntity.Mut<ReachableSpawnSourceCandidate>() = candidate;
                return;
            }

            sourceEntity.Set(candidate);
        }

        private static void CleanupDeletedSources()
        {
            foreach (var entity in SW.Query<All<ReachableSpawnSourceCandidate>, None<SpawnSource>>().Entities())
                entity.Delete<ReachableSpawnSourceCandidate>();
        }

        private static void DeleteCandidate(SW.Entity sourceEntity)
        {
            if (sourceEntity.Has<ReachableSpawnSourceCandidate>())
                sourceEntity.Delete<ReachableSpawnSourceCandidate>();
        }

        private static bool TryResolveNavArea(float3 sourcePosition, out SW.Entity bestEntity)
        {
            bestEntity = default;

            var found = false;
            var bestPriority = int.MinValue;
            var bestDistanceSq = float.MaxValue;

            foreach (var entity in SW.Query<All<NavInterestArea, RuntimeNavMeshZoneState, CombatCellPerformanceBudget, NavWorkBudgetCounter>>().Entities())
            {
                ref readonly var navArea = ref entity.Read<NavInterestArea>();
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
    }
}
