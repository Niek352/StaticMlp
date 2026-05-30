using Aspid.StaticEcs.Windows;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementContextPanelCompositeBridgeSystem
        : EcsWindowPresentationBridgeSystem<ClientCoreWT, SettlementContextPanelWindow, SettlementContextPanelSlot, SettlementContextPanelViewModel>
    {
        protected override void SyncPresentation(SettlementContextPanelViewModel viewModel)
        {
            var session = CW.GetResource<SettlementContextPanelSession>();
            var building = BuildingContextPanelPresentation.Build(in session);
            var worker = WorkerContextPanelPresentation.Build(in session);

            viewModel.Sync(in building, in worker, session.Mode);
        }
    }
}
