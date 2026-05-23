using Code.EcsUi.Mvc;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementContextPanelCompositeBridgeSystem : ControllerEcsBridgeSystem<SettlementContextPanelController>
    {
        protected override void SyncPresentation()
        {
            var session = CW.GetResource<SettlementContextPanelSession>();
            var building = BuildingContextPanelPresentation.Build(in session);
            var worker = WorkerContextPanelPresentation.Build(in session);

            Controller.Apply(in building, in worker, session.Mode);
        }
    }
}
