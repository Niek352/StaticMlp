using FFS.Libraries.StaticEcs;
using StaticMlp.Features.CombatDirector;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class ResolvedSpawnPointSystem : ISystem
    {
        public void Update()
        {
            foreach (var sourceEntity in SW.Query<All<SpawnSource, SpawnSourceNavState, ReachableSpawnSourceCandidate>>().Entities())
                UpdateResolvedSpawnPoint(sourceEntity);

            CleanupMissingCandidates();
            CleanupDeletedSources();
        }

        private static void UpdateResolvedSpawnPoint(SW.Entity sourceEntity)
        {
            ref readonly var spawnSource = ref sourceEntity.Read<SpawnSource>();
            ref readonly var navState = ref sourceEntity.Read<SpawnSourceNavState>();
            ref readonly var candidate = ref sourceEntity.Read<ReachableSpawnSourceCandidate>();

            if (!spawnSource.IsActive || navState.Status != SpawnSourceNavStatus.Reachable || navState.NavVersion != candidate.NavVersion)
            {
                DeleteResolvedSpawnPoint(sourceEntity);
                return;
            }

            if (!TryResolveNavArea(candidate.ZoneId, out var navAreaEntity))
            {
                DeleteResolvedSpawnPoint(sourceEntity);
                return;
            }

            ref readonly var navArea = ref navAreaEntity.Read<CombatCellNavArea>();
            ref readonly var zoneState = ref navAreaEntity.Read<RuntimeNavMeshZoneState>();
            if (zoneState.NavVersion != candidate.NavVersion)
            {
                DeleteResolvedSpawnPoint(sourceEntity);
                return;
            }

            var resolver = SW.GetResource<ISpawnSourcePointResolver>();
            var position = resolver.Resolve(in spawnSource, in navArea, candidate.NavVersion);

            var resolvedPoint = new ResolvedSpawnPoint
            {
                Source = sourceEntity.GID,
                Position = position,
                ZoneId = candidate.ZoneId,
                NavVersion = candidate.NavVersion
            };

            if (sourceEntity.Has<ResolvedSpawnPoint>())
            {
                sourceEntity.Mut<ResolvedSpawnPoint>() = resolvedPoint;
                return;
            }

            sourceEntity.Set(resolvedPoint);
        }

        private static void CleanupMissingCandidates()
        {
            foreach (var entity in SW.Query<All<ResolvedSpawnPoint>, None<ReachableSpawnSourceCandidate>>().Entities())
                entity.Delete<ResolvedSpawnPoint>();
        }

        private static void CleanupDeletedSources()
        {
            foreach (var entity in SW.Query<All<ResolvedSpawnPoint>, None<SpawnSource>>().Entities())
                entity.Delete<ResolvedSpawnPoint>();
        }

        private static void DeleteResolvedSpawnPoint(SW.Entity sourceEntity)
        {
            if (sourceEntity.Has<ResolvedSpawnPoint>())
                sourceEntity.Delete<ResolvedSpawnPoint>();
        }

        private static bool TryResolveNavArea(int zoneId, out SW.Entity navAreaEntity)
        {
            foreach (var entity in SW.Query<All<CombatCellNavArea, RuntimeNavMeshZoneState>>().Entities())
            {
                if (entity.Read<CombatCellNavArea>().CellId != zoneId)
                    continue;

                navAreaEntity = entity;
                return true;
            }

            navAreaEntity = default;
            return false;
        }
    }
}
