using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Input;
using StaticMlp.Networking;

namespace StaticMlp.Features.Input
{
    public sealed class ClientInputEventPublishSystem : ISystem
    {
        public void Update()
        {
            var inputState = CW.GetResource<ClientInputState>();

            for (var i = 0; i < inputState.ActionCount; i++)
            {
                if (!inputState.IsButtonAction(i))
                    continue;

                var action = inputState.GetActionName(i);

                if (inputState.WasPressedAt(i))
                    CW.SendEvent(new InputActionPressedEvent(action));

                if (inputState.WasReleasedAt(i))
                    CW.SendEvent(new InputActionReleasedEvent(action));
            }
        }
    }
}
