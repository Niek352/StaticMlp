using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Combat
{
    public sealed class ClientCombatVisualLifetimeSystem : ISystem
    {
        public void Update()
        {
            var deltaTime = Time.deltaTime;

            foreach (var entity in CW.Query<All<CombatProjectileVisualState>>().Entities())
            {
                ref var state = ref entity.Mut<CombatProjectileVisualState>();
                state.RemainingLifetime -= deltaTime;
                if (state.RemainingLifetime > 0f)
                    continue;

                if (state.ImpactEffectType != CombatEffectVisualType.None)
                {
                    ClientCombatVisualSpawnSystem.SpawnEffect(
                        state.ImpactEffectType,
                        state.ImpactPosition,
                        state.ImpactRadius,
                        state.ImpactLifetime);
                }

                entity.Destroy();
            }

            foreach (var entity in CW.Query<All<CombatEffectVisualState>>().Entities())
            {
                ref var state = ref entity.Mut<CombatEffectVisualState>();
                state.RemainingLifetime -= deltaTime;
                if (state.RemainingLifetime <= 0f)
                    entity.Destroy();
            }
        }
    }
}
