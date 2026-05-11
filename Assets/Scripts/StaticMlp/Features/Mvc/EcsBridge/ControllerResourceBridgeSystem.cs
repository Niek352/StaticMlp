using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace Code.EcsUi.Mvc
{
    public sealed class ControllerResourceBridgeSystem<TController, TState>
        : ControllerEcsBridgeSystem<TController>
        where TState : struct, IResource where TController : class, IController
    {
        private readonly Action<TController, TState> _apply;

        public ControllerResourceBridgeSystem(Action<TController, TState> apply)
        {
            _apply = apply ?? throw new ArgumentNullException(nameof(apply));
        }

        protected override void SyncPresentation()
        {
            _apply(Controller, CW.GetResource<TState>());
        }
    }
}
