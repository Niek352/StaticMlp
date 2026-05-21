using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.CombatDirector;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class NavInterestAreaCombatCellSyncSystem : ISystem
    {
        private readonly List<EntityGID> _staleNavAreas = new();

        public void Update()
        {
            foreach (var entity in SW.Query<All<CombatCell>>().Entities())
                SyncNavArea(entity);

            foreach (var entity in SW.Query<All<NavInterestArea>, None<CombatCell>>().Entities())
            {
                ref readonly var navArea = ref entity.Read<NavInterestArea>();
                if (navArea.Kind == NavInterestAreaKind.CombatCell)
                    _staleNavAreas.Add(entity.GID);
            }

            for (var i = 0; i < _staleNavAreas.Count; i++)
            {
                if (!_staleNavAreas[i].TryUnpack<ServerWT>(out var entity))
                    continue;

                if (entity.Has<NavInterestArea>())
                {
                    ref readonly var navArea = ref entity.Read<NavInterestArea>();
                    if (navArea.Kind != NavInterestAreaKind.CombatCell)
                        continue;
                }

                CleanupEntityNavState(entity);
            }

            _staleNavAreas.Clear();
        }

        private static void SyncNavArea(SW.Entity entity)
        {
            ref readonly var combatCell = ref entity.Read<CombatCell>();
            var navArea = NavInterestAreaRules.CreateFromCombatCell(in combatCell);

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
    }
}
