using FFS.Libraries.StaticEcs;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Features.Shared;
using UnityEngine;

namespace StaticMlp.Features.Combat
{
    public sealed class ClientDamageFeedbackPresentationSystem : ISystem
    {
        public void Update()
        {
            var config = CW.GetResource<CombatPresentationConfig>();
            DecayExistingFeedback(config, CW.GetResource<GameTime>().DeltaTime);
            ApplyNewFeedback(config);
        }

        private static void DecayExistingFeedback(CombatPresentationConfig config, float deltaTime)
        {
            foreach (var entity in CW.Query<All<DamageFeedbackViewState>>().Entities())
            {
                ref var state = ref entity.Mut<DamageFeedbackViewState>();
                if (!state.IsActive)
                    continue;

                state.RemainingLifetime -= deltaTime;
                if (state.RemainingLifetime <= 0f || config.DamageFlashLifetime <= 0f)
                {
                    state.RemainingLifetime = 0f;
                    state.Intensity = 0f;
                    state.IsActive = false;
                    continue;
                }

                state.Intensity = Mathf.Clamp01(state.RemainingLifetime / config.DamageFlashLifetime);
            }
        }

        private static void ApplyNewFeedback(CombatPresentationConfig config)
        {
            foreach (var entity in CW.Query<All<CharacterNetState, Health>>().Entities())
            {
                ref readonly var health = ref entity.Read<Health>();
                if (!entity.Has<HealthPresentationState>())
                {
                    entity.Set(new HealthPresentationState
                    {
                        LastKnownCurrent = health.Current,
                        IsInitialized = true
                    });
                    continue;
                }

                ref var presentation = ref entity.Mut<HealthPresentationState>();
                if (!presentation.IsInitialized)
                {
                    presentation.LastKnownCurrent = health.Current;
                    presentation.IsInitialized = true;
                    continue;
                }

                var previous = presentation.LastKnownCurrent;
                presentation.LastKnownCurrent = health.Current;

                var damageAmount = previous - health.Current;
                if (damageAmount <= 0.009f)
                    continue;

                entity.Set(new DamageFeedbackViewState
                {
                    IsActive = true,
                    DamageAmount = damageAmount,
                    RemainingLifetime = Mathf.Max(0f, config.DamageFlashLifetime),
                    Intensity = 1f
                });
            }
        }
    }
}
