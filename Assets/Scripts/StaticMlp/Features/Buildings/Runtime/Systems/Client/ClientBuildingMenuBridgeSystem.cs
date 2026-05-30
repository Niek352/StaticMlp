using Aspid.StaticEcs.Windows;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    public sealed class ClientBuildingMenuBridgeSystem
        : EcsWindowPresentationBridgeSystem<ClientCoreWT, BuildingMenuWindow, BuildingMenuSlot, BuildingMenuViewModel>
    {
        protected override void SyncPresentation(BuildingMenuViewModel viewModel)
        {
            ref readonly var state = ref CW.GetResource<BuildingMenuState>();
            viewModel.Sync(in state);
        }
    }
}
