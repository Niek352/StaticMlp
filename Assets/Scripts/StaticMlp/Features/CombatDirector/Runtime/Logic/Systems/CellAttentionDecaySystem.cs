using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game;
using StaticMlp.Networking;
using Unity.Mathematics;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class CellAttentionDecaySystem : ISystem
    {
        public void Update()
        {
            var deltaTime = SW.GetResource<SimulationTime>().FixedStepSeconds;
            var directorEntity = ReadDirectorEntity();
            ref var attention = ref directorEntity.Mut<CellAttention>();
            ref var budget = ref directorEntity.Mut<ThreatBudget>();

            if (attention.DecayPerSecond < 0f)
                throw new InvalidOperationException("Cell attention decay must be non-negative.");

            if (deltaTime < 0f)
                throw new InvalidOperationException("Simulation fixed step must be non-negative.");

            var total = attention.Noise + attention.Trespass + attention.Combat + attention.Loot + attention.FactionAlarm;
            if (total <= 0f)
            {
                attention.Noise = 0f;
                attention.Trespass = 0f;
                attention.Combat = 0f;
                attention.Loot = 0f;
                attention.FactionAlarm = 0f;
                attention.Current = 0f;
                budget.Current = 0f;
                budget.Max = attention.Max;
                budget.AccumulationPerSecond = 0f;
                return;
            }

            var decay = math.min(total, attention.DecayPerSecond * deltaTime);
            var remainingScale = (total - decay) / total;
            attention.Noise *= remainingScale;
            attention.Trespass *= remainingScale;
            attention.Combat *= remainingScale;
            attention.Loot *= remainingScale;
            attention.FactionAlarm *= remainingScale;
            attention.Current = math.max(
                0f,
                math.min(attention.Max, attention.Noise + attention.Trespass + attention.Combat + attention.Loot + attention.FactionAlarm));

            budget.Current = attention.Current;
            budget.Max = attention.Max;
            budget.AccumulationPerSecond = 0f;
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
                throw new InvalidOperationException("Combat Director cell entity must exist before attention decay.");

            return directorEntity;
        }
    }
}
