using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Frontier
{
    public static class ExpeditionHudPresentation
    {
        public static ExpeditionHudState Build()
        {
            if (!Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
                return default;

            var state = new ExpeditionHudState();

            if (anchor.Has<Projected<ExpeditionAvailabilityState>>())
                state.Availability = ClientProjection.Read<ExpeditionAvailabilityState>(anchor).Status;

            if (anchor.Has<Projected<ActiveExpeditionState>>())
                state.Activity = ClientProjection.Read<ActiveExpeditionState>(anchor).Status;

            return state;
        }
    }
}
