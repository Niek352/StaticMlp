using Code.EcsUi.Mvc;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Frontier
{
    public sealed class ThreatBannerBridgeSystem : ControllerEcsBridgeSystem<ThreatBannerController>
    {
        protected override void SyncPresentation()
        {
            var state = new ThreatBannerState
            {
                IsVisible = true,
                Phase = ThreatPhase.Calm,
            };

            if (Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
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

            Controller.Apply(in state);
        }
    }
}
