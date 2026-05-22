using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace Code.EcsUi.Mvc
{
    [Obsolete("Temp: Use a custom ControllerEcsBridgeSystem<TController> that reads components directly.")]
    public sealed class ControllerResourceBridgeSystem<TController, TState>
        : ControllerEcsBridgeSystem<TController>
        where TState : struct, IResource
        where TController : class, IController, IResourcePresentationController<TState>
    {
        protected override void SyncPresentation()
        {
            ref readonly var state = ref CW.GetResource<TState>();
            Controller.Apply(in state);
        }
    }
}
