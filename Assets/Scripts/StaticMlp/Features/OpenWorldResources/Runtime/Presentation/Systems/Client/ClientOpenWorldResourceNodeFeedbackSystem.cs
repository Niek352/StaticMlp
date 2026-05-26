using FFS.Libraries.StaticEcs;
using StaticMlp.Game;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class ClientOpenWorldResourceNodeFeedbackSystem : ISystem
    {
        private const float HIT_FLASH_DECAY_PER_SECOND = 5.5f;
        private const float DEPLETION_PULSE_DECAY_PER_SECOND = 2.5f;

        public void Update()
        {
            var deltaTime = CW.GetResource<GameTime>().DeltaTime;
            foreach (var entity in CW.Query<All<OpenWorldResourceNodeViewState>>().Entities())
            {
                ref var state = ref entity.Mut<OpenWorldResourceNodeViewState>();
                UpdateFeedback(ref state, deltaTime);
            }
        }

        private static void UpdateFeedback(ref OpenWorldResourceNodeViewState state, float deltaTime)
        {
            if (state.PreviousRemainingAmount == 0 && state.RemainingAmount > 0)
                state.PreviousRemainingAmount = state.RemainingAmount;

            if (state.PreviousRemainingAmount > state.RemainingAmount)
                state.HitFlashIntensity = 1f;

            if (state.PreviousRemainingAmount > 0 && IsDepleted(state))
                state.DepletionPulseIntensity = 1f;

            state.PreviousRemainingAmount = state.RemainingAmount;
            state.HitFlashIntensity = Mathf.Max(0f, state.HitFlashIntensity - HIT_FLASH_DECAY_PER_SECOND * deltaTime);
            state.DepletionPulseIntensity = Mathf.Max(0f, state.DepletionPulseIntensity - DEPLETION_PULSE_DECAY_PER_SECOND * deltaTime);
        }

        private static bool IsDepleted(OpenWorldResourceNodeViewState state)
        {
            return state.RemainingAmount <= 0
                   || (state.Flags & OpenWorldResourceOverlayFlags.Depleted) != 0;
        }
    }
}
