using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Frontier
{
    public sealed class ClientThreatBannerStateSystem : ISystem
    {
        public void Update()
        {
            var next = new ThreatBannerState
            {
                IsVisible = true,
                Phase = ThreatPhase.Calm,
            };

            if (Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
            {
                if (anchor.Has<Projected<ThreatState>>())
                    next.Phase = ClientProjection.Read<ThreatState>(anchor).Phase;

                if (anchor.Has<Projected<RaidScheduleState>>())
                {
                    ref readonly var raid = ref ClientProjection.Read<RaidScheduleState>(anchor);
                    next.RaidStatus = raid.Status;
                    next.ActivateAtTick = raid.ActivateAtTick;
                }
            }

            CW.SetResource(next);
        }
    }
}
