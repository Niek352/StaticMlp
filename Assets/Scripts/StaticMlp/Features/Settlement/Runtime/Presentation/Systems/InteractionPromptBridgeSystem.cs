using Aspid.StaticEcs.Windows;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class InteractionPromptBridgeSystem
        : EcsWindowPresentationBridgeSystem<ClientCoreWT, InteractionPromptWindow, InteractionPromptSlot, InteractionPromptViewModel>
    {
        protected override void SyncPresentation(InteractionPromptViewModel viewModel)
        {
            ref readonly var state = ref CW.GetResource<InteractionPromptState>();
            viewModel.Sync(in state);
        }
    }
}
