using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using Unity.Mathematics;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class SpawnRequestBuildSystem : ISystem
    {
        public void Update()
        {
            var directorEntity = ReadDirectorEntity();
            ref readonly var state = ref directorEntity.Read<DirectorState>();
            if (!IsSpawnPhase(state.Phase) || !directorEntity.Has<SelectedSpawnSource>() || HasPendingSpawnRequest())
                return;

            var config = SW.GetResource<EncounterDirectorConfig>();
            ref readonly var cell = ref directorEntity.Read<CombatCell>();
            ref readonly var budget = ref directorEntity.Read<ThreatBudget>();
            ref readonly var selectedSource = ref directorEntity.Read<SelectedSpawnSource>();

            var aliveCount = CountAliveEnemies(in cell);
            var remainingSlots = config.MaxAliveEnemiesPerCell - aliveCount;
            if (remainingSlots <= 0)
                return;

            var remainingBudget = budget.Current;
            if (remainingBudget <= 0f)
                return;

            var catalog = SW.GetResource<EnemySpawnCatalog>();
            BuildRequests(in selectedSource, catalog, config, budget.Current, ref remainingBudget, ref remainingSlots);
        }

        private static void BuildRequests(
            in SelectedSpawnSource selectedSource,
            EnemySpawnCatalog catalog,
            EncounterDirectorConfig config,
            float currentBudget,
            ref float remainingBudget,
            ref int remainingSlots)
        {
            if (currentBudget >= config.PeakThreshold)
            {
                AddRequest(in selectedSource, catalog.Get(EnemyRole.AnchorElite), minCount: 1, maxCount: 1, ref remainingBudget, ref remainingSlots);
                AddRequest(in selectedSource, catalog.Get(EnemyRole.Marker), minCount: 1, maxCount: 1, ref remainingBudget, ref remainingSlots);
                AddRequest(in selectedSource, catalog.Get(EnemyRole.Swarmer), minCount: 10, maxCount: 18, ref remainingBudget, ref remainingSlots);
                return;
            }

            var mediumThreshold = (config.BuildUpThreshold + config.PeakThreshold) * 0.5f;
            if (currentBudget >= mediumThreshold)
            {
                AddRequest(in selectedSource, catalog.Get(EnemyRole.Marker), minCount: 1, maxCount: 1, ref remainingBudget, ref remainingSlots);
                AddRequest(in selectedSource, catalog.Get(EnemyRole.Swarmer), minCount: 8, maxCount: 14, ref remainingBudget, ref remainingSlots);
                return;
            }

            AddRequest(in selectedSource, catalog.Get(EnemyRole.Swarmer), minCount: 6, maxCount: 10, ref remainingBudget, ref remainingSlots);
        }

        private static void AddRequest(
            in SelectedSpawnSource selectedSource,
            in EnemySpawnDefinition definition,
            int minCount,
            int maxCount,
            ref float remainingBudget,
            ref int remainingSlots)
        {
            if (remainingSlots <= 0)
                return;

            var affordableCount = (int)math.floor(remainingBudget / definition.BudgetCost);
            var count = math.min(math.min(maxCount, affordableCount), remainingSlots);
            if (count < minCount)
                return;

            remainingBudget -= count * definition.BudgetCost;
            remainingSlots -= count;

            SW.NewEntity<Default>().Set(new SpawnRequest
            {
                CellId = selectedSource.CellId,
                SourceEntity = selectedSource.SourceEntity,
                SourceType = selectedSource.SourceType,
                Role = definition.Role,
                Count = count,
                SpawnPosition = selectedSource.Position
            });
        }

        private static SW.Entity ReadDirectorEntity()
        {
            var found = false;
            SW.Entity directorEntity = default;

            foreach (var entity in SW.Query<All<CombatCell, ThreatBudget, DirectorState>>().Entities())
            {
                if (found)
                    throw new InvalidOperationException("Combat Director currently supports only a single director cell.");

                directorEntity = entity;
                found = true;
            }

            if (!found)
                throw new InvalidOperationException("Combat Director cell entity must exist before spawn request building.");

            return directorEntity;
        }

        private static bool HasPendingSpawnRequest()
        {
            foreach (var _ in SW.Query<All<SpawnRequest>>().Entities())
                return true;

            return false;
        }

        private static bool IsSpawnPhase(DirectorPhase phase)
        {
            return phase == DirectorPhase.BuildUp || phase == DirectorPhase.Peak;
        }

        private static int CountAliveEnemies(in CombatCell cell)
        {
            var aliveEnemyCount = 0;

            foreach (var entity in SW.Query<All<EnemyTag, CharacterNetState>, None<IsDiedTag>>().Entities())
            {
                var position = ToFloat3(entity.Read<CharacterNetState>().Position);
                if (!math.all(math.isfinite(position)))
                    throw new InvalidOperationException("Enemy position must be finite.");

                if (math.distancesq(position, cell.Center) > cell.Radius * cell.Radius)
                    continue;

                aliveEnemyCount++;
            }

            return aliveEnemyCount;
        }

        private static float3 ToFloat3(UnityEngine.Vector3 value)
        {
            return new float3(value.x, value.y, value.z);
        }
    }
}
