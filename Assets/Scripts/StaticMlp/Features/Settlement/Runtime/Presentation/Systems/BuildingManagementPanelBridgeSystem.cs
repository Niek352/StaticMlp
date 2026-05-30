using Aspid.StaticEcs.Windows;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class BuildingManagementPanelBridgeSystem
        : EcsWindowPresentationBridgeSystem<ClientCoreWT, BuildingManagementPanelWindow, BuildingManagementPanelSlot, BuildingManagementPanelViewModel>
    {
        protected override void SyncPresentation(BuildingManagementPanelViewModel viewModel)
        {
            ref readonly var session = ref CW.GetResource<BuildingPanelSession>();
            var state = BuildingPanelPresentation.Build(in session);
            viewModel.Sync(in state);
        }
    }
}
