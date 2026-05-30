using Aspid.StaticEcs.Windows;
using StaticMlp.Networking;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public sealed class ResourcesInventoryHudBridgeSystem
        : EcsWindowPresentationBridgeSystem<ClientCoreWT, ResourcesInventoryHudWindow, ResourcesInventoryHudSlot, ResourcesInventoryHudViewModel>
    {
        protected override void SyncPresentation(ResourcesInventoryHudViewModel viewModel)
        {
            var presentation = ResourcesInventoryHudPresentationBuilder.Build();
            viewModel.Sync(in presentation);
        }
    }
}
