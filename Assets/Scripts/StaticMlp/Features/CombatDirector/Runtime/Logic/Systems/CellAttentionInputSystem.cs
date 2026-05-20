using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using Unity.Mathematics;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class CellAttentionInputSystem : ISystem
    {
        public void Update()
        {
            var config = SW.GetResource<EncounterDirectorConfig>();
            var deltaTime = SW.GetResource<SimulationTime>().FixedStepSeconds;
            var directorEntity = ReadDirectorEntity();
            ref readonly var cell = ref directorEntity.Read<CombatCell>();
            ref var attention = ref directorEntity.Mut<CellAttention>();
            ref var budget = ref directorEntity.Mut<ThreatBudget>();

            ValidateConfig(config, in attention);
            if (deltaTime < 0f)
                throw new InvalidOperationException("Simulation fixed step must be non-negative.");

            var noise = 0f;
            var combat = 0f;
            var loot = 0f;
            foreach (var player in SW.Query<All<PlayerTag, CharacterNetState, PlayerNoise, PlayerCombatAttention, CarriedLootValue>>().Entities())
            {
                var position = ToFloat3(player.Read<CharacterNetState>().Position);
                if (!IsInsideCell(position, in cell))
                    continue;

                noise += player.Read<PlayerNoise>().Value * config.NoiseAttentionMultiplier;
                combat += player.Read<PlayerCombatAttention>().Value * config.CombatAttentionMultiplier;
                loot += player.Read<CarriedLootValue>().Value * config.LootAttentionMultiplier * deltaTime;
            }

            if (noise < 0f || combat < 0f || loot < 0f)
                throw new InvalidOperationException("Cell attention inputs must be non-negative.");

            attention.Noise = math.min(attention.Max, attention.Noise + noise);
            attention.Combat = math.min(attention.Max, attention.Combat + combat);
            attention.Loot = math.min(attention.Max, attention.Loot + loot);
            attention.Current = math.min(
                attention.Max,
                attention.Noise + attention.Trespass + attention.Combat + attention.Loot + attention.FactionAlarm);

            budget.Current = attention.Current;
            budget.Max = attention.Max;
            budget.AccumulationPerSecond = deltaTime > 0f
                ? (noise + combat) / deltaTime + loot / deltaTime
                : 0f;
        }

        private static void ValidateConfig(EncounterDirectorConfig config, in CellAttention attention)
        {
            if (config.NoiseAttentionMultiplier < 0f
                || config.CombatAttentionMultiplier < 0f
                || config.LootAttentionMultiplier < 0f
                || config.TrespassAttentionMultiplier < 0f
                || config.FactionAlarmAttentionMultiplier < 0f)
            {
                throw new InvalidOperationException("EncounterDirectorConfig attention multipliers must be non-negative.");
            }

            if (attention.Max < config.PeakThreshold)
                throw new InvalidOperationException("Cell attention max must be at least the peak threshold.");
        }

        private static SW.Entity ReadDirectorEntity()
        {
            var found = false;
            SW.Entity directorEntity = default;

            foreach (var entity in SW.Query<All<CombatCell, CellAttention, ThreatBudget, DirectorState>>().Entities())
            {
                if (found)
                    throw new InvalidOperationException("Combat Director currently supports only a single director cell.");

                directorEntity = entity;
                found = true;
            }

            if (!found)
                throw new InvalidOperationException("Combat Director cell entity must exist before attention input updates.");

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
