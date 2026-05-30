using System;
using StaticMlp.Features.CampFlow;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Frontier
{
    public sealed class ThreatBannerBridgeSystem : ISystem
    {
        public void Update()
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

            foreach (var entity in CW.Query<All<ThreatBannerViewData>>().Entities())
            {
                ref var data = ref entity.Mut<ThreatBannerViewData>();
                data.State = state;
                return;
            }

            throw new InvalidOperationException($"{nameof(ThreatBannerViewData)} entity is missing.");
        }
    }
}
