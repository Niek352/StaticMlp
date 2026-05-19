using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Frontier;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiNavigation
{
    public sealed class CombatCellNavAreaSyncSystem : ISystem
    {
        private readonly List<EntityGID> _staleNavAreas = new();

        public void Update()
        {
            foreach (var entity in SW.Query<All<CombatCell>>().Entities())
                SyncNavArea(entity);

            foreach (var entity in SW.Query<All<CombatCellNavArea>, None<CombatCell>>().Entities())
                _staleNavAreas.Add(entity.GID);

            for (var i = 0; i < _staleNavAreas.Count; i++)
            {
                if (!_staleNavAreas[i].TryUnpack<ServerWT>(out var entity))
                    continue;

                if (entity.Has<NavRebuildRequest>())
                    entity.Delete<NavRebuildRequest>();

                if (entity.Has<RuntimeNavMeshZoneState>())
                    entity.Delete<RuntimeNavMeshZoneState>();

                if (entity.Has<NavWorkBudgetCounter>())
                    entity.Delete<NavWorkBudgetCounter>();

                if (entity.Has<CombatCellPerformanceBudget>())
                    entity.Delete<CombatCellPerformanceBudget>();

                if (entity.Has<CombatCellNavArea>())
                    entity.Delete<CombatCellNavArea>();
            }

            _staleNavAreas.Clear();
        }

        private static void SyncNavArea(SW.Entity entity)
        {
            ref readonly var combatCell = ref entity.Read<CombatCell>();
            var navArea = CombatCellNavAreaRules.Create(in combatCell);

            if (entity.Has<CombatCellNavArea>())
            {
                ref var currentNavArea = ref entity.Mut<CombatCellNavArea>();
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
    }
}
