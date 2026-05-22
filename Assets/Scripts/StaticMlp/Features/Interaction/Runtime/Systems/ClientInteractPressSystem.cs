using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Input;
using StaticMlp.Networking;

namespace StaticMlp.Features.Interaction
{
    /// <summary>
    /// Fires <see cref="InteractPressedEvent"/> when the player presses the Interact key
    /// and <see cref="InteractionFocus"/> has a valid target.
    /// Must run after <see cref="ClientInteractionFocusSystem"/> each frame.
    /// </summary>
    public sealed class ClientInteractPressSystem : ISystem
    {
        public void Update()
        {
            var inputState = CW.GetResource<ClientInputState>();
            if (!inputState.WasPressed(CoreInputActions.Interact))
                return;

            ref readonly var focus = ref CW.GetResource<InteractionFocus>();
            if (!focus.HasFocus)
                return;

            CW.SendEvent(new InteractPressedEvent
            {
                Target = focus.Target,
                Kind   = focus.Kind,
            });
        }
    }
}
