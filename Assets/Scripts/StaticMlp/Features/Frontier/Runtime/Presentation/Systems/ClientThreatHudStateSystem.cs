using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Frontier
{
    public sealed class ClientThreatHudStateSystem : ISystem
    {
        public void Update()
        {
            if (!Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
                return;

            var next = new ThreatHudState();

            if (anchor.Has<Projected<ThreatState>>())
                next.ThreatPhase = ClientProjection.Read<ThreatState>(anchor).Phase;

            CW.SetResource(next);
        }
    }
}
