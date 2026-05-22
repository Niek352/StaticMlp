using Code.EcsUi.Mvc;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class Stage1ContextPanelCompositeBridgeSystem : ControllerEcsBridgeSystem<Stage1ContextPanelController>
    {
        protected override void SyncPresentation()
        {
            var session = CW.GetResource<Stage1ContextPanelSession>();
            var building = BuildingContextPanelPresentation.Build(in session);
            var worker = WorkerContextPanelPresentation.Build(in session);

            Controller.Apply(in building, in worker, session.Mode);
        }
    }
}
