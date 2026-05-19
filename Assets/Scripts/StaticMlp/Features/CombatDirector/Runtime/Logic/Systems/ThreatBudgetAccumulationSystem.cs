using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using Unity.Mathematics;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class ThreatBudgetAccumulationSystem : ISystem
    {
        public void Update()
        {
            var config = SW.GetResource<EncounterDirectorConfig>();
            var deltaTime = SW.GetResource<SimulationTime>().FixedStepSeconds;
            var directorEntity = ReadDirectorEntity();
            ref readonly var cell = ref directorEntity.Read<CombatCell>();
            ref var budget = ref directorEntity.Mut<ThreatBudget>();

            var totalNoise = 0f;
            var totalCarriedLootValue = 0f;
            foreach (var player in SW.Query<All<PlayerTag, CharacterNetState, PlayerNoise, CarriedLootValue>>().Entities())
            {
                var position = ToFloat3(player.Read<CharacterNetState>().Position);
                if (!IsInsideCell(position, in cell))
                    continue;

                totalNoise += player.Read<PlayerNoise>().Value;
                totalCarriedLootValue += player.Read<CarriedLootValue>().Value;
            }

            const float activeObjectiveBonus = 0f;
            var threatDeltaPerSecond =
                config.BaseThreatPerSecond
                + totalNoise * config.NoiseThreatMultiplier
                + totalCarriedLootValue * config.LootThreatMultiplier
                + activeObjectiveBonus;

            if (threatDeltaPerSecond < 0f)
                throw new InvalidOperationException("Threat delta per second must be non-negative.");

            if (budget.Max < config.PeakThreshold)
                throw new InvalidOperationException("Threat budget max must be at least the peak threshold.");

            budget.AccumulationPerSecond = threatDeltaPerSecond;
            budget.Current = math.min(budget.Max, budget.Current + threatDeltaPerSecond * deltaTime);
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
                throw new InvalidOperationException("Combat Director cell entity must exist before threat budget accumulation.");

            return directorEntity;
        }

        private static bool IsInsideCell(float3 position, in CombatCell cell)
        {
            return math.distancesq(position, cell.Center) <= cell.Radius * cell.Radius;
        }

        private static float3 ToFloat3(UnityEngine.Vector3 value)
        {
            return new float3(value.x, value.y, value.z);
        }
    }
}
