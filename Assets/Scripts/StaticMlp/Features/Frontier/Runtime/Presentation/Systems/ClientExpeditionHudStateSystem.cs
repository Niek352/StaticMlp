using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Frontier
{
    public sealed class ClientExpeditionHudStateSystem : ISystem
    {
        public void Update()
        {
            if (!Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
                return;

            var next = new ExpeditionHudState();

            if (anchor.Has<Projected<ExpeditionAvailabilityState>>())
                next.Availability = ClientProjection.Read<ExpeditionAvailabilityState>(anchor).Status;

            if (anchor.Has<Projected<ActiveExpeditionState>>())
                next.Activity = ClientProjection.Read<ActiveExpeditionState>(anchor).Status;

            CW.SetResource(next);
        }
    }
}
