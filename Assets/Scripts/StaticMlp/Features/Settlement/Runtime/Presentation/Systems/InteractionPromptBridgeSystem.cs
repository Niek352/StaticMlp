using Code.EcsUi.Mvc;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class InteractionPromptBridgeSystem : ControllerEcsBridgeSystem<InteractionPromptController>
    {
        protected override void SyncPresentation()
        {
            ref readonly var state = ref CW.GetResource<InteractionPromptState>();
            Controller.Apply(in state);
        }
    }
}
