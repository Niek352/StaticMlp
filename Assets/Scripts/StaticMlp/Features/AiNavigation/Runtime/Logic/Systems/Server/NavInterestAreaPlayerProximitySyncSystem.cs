using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class NavInterestAreaPlayerProximitySyncSystem : ISystem
    {
        private const float PROXIMITY_RADIUS = 120f;
        private const float PROXIMITY_NAV_BUILD_RADIUS_MULTIPLIER = 1.6f;
        private const float PROXIMITY_SOURCE_COLLECT_RADIUS_MULTIPLIER = 2f;
        private const int PROXIMITY_PRIORITY = 1500;
        private const float NAV_SECTOR_SIZE = 16f;

        private readonly List<EntityGID> _staleNavAreas = new();

        public void Update()
        {
            foreach (var entity in SW.Query<All<PlayerTag, CharacterNetState>>().Entities())
                SyncNavArea(entity);

            foreach (var entity in SW.Query<All<NavInterestArea>, None<PlayerTag>>().Entities())
            {
                ref readonly var navArea = ref entity.Read<NavInterestArea>();
                if (navArea.Kind == NavInterestAreaKind.PlayerProximity)
                    _staleNavAreas.Add(entity.GID);
            }

            for (var i = 0; i < _staleNavAreas.Count; i++)
            {
                if (!_staleNavAreas[i].TryUnpack<ServerWT>(out var entity))
                    continue;

                if (entity.Has<NavInterestArea>())
                {
                    ref readonly var navArea = ref entity.Read<NavInterestArea>();
                    if (navArea.Kind != NavInterestAreaKind.PlayerProximity)
                        continue;
                }

                CleanupEntityNavState(entity);
            }

            _staleNavAreas.Clear();
        }

        private static void SyncNavArea(SW.Entity entity)
        {
            ref readonly var characterState = ref entity.Read<CharacterNetState>();

            var center = new float3(characterState.Position.x, characterState.Position.y, characterState.Position.z);
            var quantizedCenter = QuantizeNavCenter(center);

            var navBuildRadius = math.max(PROXIMITY_RADIUS, PROXIMITY_RADIUS * PROXIMITY_NAV_BUILD_RADIUS_MULTIPLIER);
            var sourceCollectRadius = math.max(PROXIMITY_RADIUS * PROXIMITY_NAV_BUILD_RADIUS_MULTIPLIER, PROXIMITY_RADIUS * PROXIMITY_SOURCE_COLLECT_RADIUS_MULTIPLIER);

            var navArea = new NavInterestArea
            {
                AreaId = entity.GID.GetHashCode(),
                Kind = NavInterestAreaKind.PlayerProximity,
                Center = quantizedCenter,
                Radius = PROXIMITY_RADIUS,
                NavBuildRadius = navBuildRadius,
                SourceCollectRadius = sourceCollectRadius,
                Priority = PROXIMITY_PRIORITY
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
