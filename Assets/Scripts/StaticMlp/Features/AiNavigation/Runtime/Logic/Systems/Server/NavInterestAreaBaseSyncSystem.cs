using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class NavInterestAreaBaseSyncSystem : ISystem
    {
        private const float BASE_RADIUS = 60f;
        private const float BASE_NAV_BUILD_RADIUS_MULTIPLIER = 1.6f;
        private const float BASE_SOURCE_COLLECT_RADIUS_MULTIPLIER = 2f;
        private const int BASE_PRIORITY = 2000;
        private const float NAV_SECTOR_SIZE = 16f;

        private readonly List<EntityGID> _staleNavAreas = new();

        public void Update()
        {
            foreach (var entity in SW.Query<All<SettlementAnchorRef, SettlementAnchorLocation>>().Entities())
                SyncNavArea(entity);

            foreach (var entity in SW.Query<All<NavInterestArea>, None<SettlementAnchorRef>>().Entities())
            {
                ref readonly var navArea = ref entity.Read<NavInterestArea>();
                if (navArea.Kind == NavInterestAreaKind.PlayerBase)
                    _staleNavAreas.Add(entity.GID);
            }

            for (var i = 0; i < _staleNavAreas.Count; i++)
            {
                if (!_staleNavAreas[i].TryUnpack<ServerWT>(out var entity))
                    continue;

                if (entity.Has<NavInterestArea>())
                {
                    ref readonly var navArea = ref entity.Read<NavInterestArea>();
                    if (navArea.Kind != NavInterestAreaKind.PlayerBase)
                        continue;
                }

                CleanupEntityNavState(entity);
            }

            _staleNavAreas.Clear();
        }

        private static void SyncNavArea(SW.Entity entity)
        {
            ref readonly var anchorRef = ref entity.Read<SettlementAnchorRef>();
            ref readonly var anchorLocation = ref entity.Read<SettlementAnchorLocation>();

            var center = new float3(anchorLocation.Position.x, anchorLocation.Position.y, anchorLocation.Position.z);
            var quantizedCenter = QuantizeNavCenter(center);

            var navBuildRadius = math.max(BASE_RADIUS, BASE_RADIUS * BASE_NAV_BUILD_RADIUS_MULTIPLIER);
            var sourceCollectRadius = math.max(BASE_RADIUS * BASE_NAV_BUILD_RADIUS_MULTIPLIER, BASE_RADIUS * BASE_SOURCE_COLLECT_RADIUS_MULTIPLIER);

            var navArea = new NavInterestArea
            {
                AreaId = anchorRef.AnchorId,
                Kind = NavInterestAreaKind.PlayerBase,
                Center = quantizedCenter,
                Radius = BASE_RADIUS,
                NavBuildRadius = navBuildRadius,
                SourceCollectRadius = sourceCollectRadius,
                Priority = BASE_PRIORITY
            };

            if (entity.Has<NavInterestArea>())
            {
                ref var currentNavArea = ref entity.Mut<NavInterestArea>();
                currentNavArea = navArea;
            }
            else
            {
                entity.Set(navArea);
            }

            if (!entity.Has<RuntimeNavMeshZoneState>())
                entity.Set(default(RuntimeNavMeshZoneState));

            if (!entity.Has<NavWorkBudgetCounter>())
                entity.Set(default(NavWorkBudgetCounter));

            if (!entity.Has<CombatCellPerformanceBudget>())
                entity.Set(CombatCellPerformanceBudgetDefaults.Create());
        }

        private static void CleanupEntityNavState(SW.Entity entity)
        {
            if (entity.Has<NavRebuildRequest>())
                entity.Delete<NavRebuildRequest>();

            if (entity.Has<RuntimeNavMeshZoneState>())
            {
                SW.GetResource<RuntimeNavMeshZoneBackend>().Remove(entity.GID);
                entity.Delete<RuntimeNavMeshZoneState>();
            }

            if (entity.Has<NavWorkBudgetCounter>())
                entity.Delete<NavWorkBudgetCounter>();

            if (entity.Has<CombatCellPerformanceBudget>())
                entity.Delete<CombatCellPerformanceBudget>();

            if (entity.Has<NavInterestArea>())
                entity.Delete<NavInterestArea>();
        }

        private static float3 QuantizeNavCenter(float3 center)
        {
            if (!math.all(math.isfinite(center)))
                throw new InvalidOperationException("Nav area center must be finite before quantization.");

            return new float3(
                math.round(center.x / NAV_SECTOR_SIZE) * NAV_SECTOR_SIZE,
                center.y,
                math.round(center.z / NAV_SECTOR_SIZE) * NAV_SECTOR_SIZE);
        }
    }
}
