using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Effects;
using StaticMlp.Networking;

namespace StaticMlp.Features.CombatDirector
{
    public sealed class SpawnSourceVfxSystem : ISystem
    {
        private const float SOURCE_EFFECT_LIFETIME = 0.45f;

        public void Update()
        {
            foreach (var telegraph in CW.Query<All<SpawnSourceTelegraphState>>().Entities())
            {
                ref readonly var state = ref telegraph.Read<SpawnSourceTelegraphState>();
                ClientCombatVisualSpawnSystem.SpawnEffect(
                    ResolveEffectType(state.SourceType),
                    state.Position,
                    state.Radius,
                    SOURCE_EFFECT_LIFETIME);
                telegraph.Destroy();
            }
        }

        private static CombatEffectVisualType ResolveEffectType(SpawnSourceType sourceType)
        {
            return sourceType switch
            {
                SpawnSourceType.Burrow => CombatEffectVisualType.FireBurst,
                SpawnSourceType.Rift => CombatEffectVisualType.PoisonBurst,
                _ => throw new InvalidOperationException($"Unsupported spawn source VFX type: {sourceType}.")
            };
        }
    }
}
