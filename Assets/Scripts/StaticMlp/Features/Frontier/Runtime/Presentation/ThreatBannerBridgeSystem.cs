using Aspid.StaticEcs.Windows;
using StaticMlp.Features.CampFlow;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Frontier
{
    public sealed class ThreatBannerBridgeSystem
        : EcsWindowPresentationBridgeSystem<ClientCoreWT, ThreatBannerWindow, ThreatBannerSlot, ThreatBannerViewModel>
    {
        protected override void SyncPresentation(ThreatBannerViewModel viewModel)
        {
            var state = new ThreatBannerState
            {
                IsVisible = true,
                Phase = ThreatPhase.Calm,
            };

            if (CampFlowProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
            {
                if (anchor.Has<Projected<ThreatState>>())
                    state.Phase = ClientProjection.Read<ThreatState>(anchor).Phase;

                if (anchor.Has<Projected<RaidScheduleState>>())
                {
                    ref readonly var raid = ref ClientProjection.Read<RaidScheduleState>(anchor);
                    state.RaidStatus = raid.Status;
                    state.ActivateAtTick = raid.ActivateAtTick;
                }
            }

            viewModel.Sync(in state);
        }
    }
}
