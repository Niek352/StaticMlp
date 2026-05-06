using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Input;
using StaticMlp.Networking;

namespace StaticMlp.Features.Input
{
    public sealed class ClientInputCaptureSystem : ISystem
    {
        private InputResource _input;

        public void Init()
        {
            _input = new InputResource();
            _input.Enable();

            var inputState = new ClientInputState();
            _input.Configure(inputState);

            CW.SetResource(_input);
            CW.SetResource(inputState);
        }

        public void Destroy()
        {
            _input?.Dispose();
            _input = null;
        }

        public void Update()
        {
            var input = CW.GetResource<InputResource>();
            var inputState = CW.GetResource<ClientInputState>();
            input.CaptureInto(inputState);
        }
    }
}
