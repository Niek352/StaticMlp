using Code.EcsUi.Mvc;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class BuildingManagementPanelBridgeSystem : ControllerEcsBridgeSystem<BuildingManagementPanelController>
    {
        protected override void SyncPresentation()
        {
            ref readonly var session = ref CW.GetResource<BuildingPanelSession>();
            var state = BuildingPanelPresentation.Build(in session);
            Controller.Apply(in state);
        }
    }
}
