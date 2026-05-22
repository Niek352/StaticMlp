using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Frontier
{
    public static class ThreatHudPresentation
    {
        public static ThreatHudState Build()
        {
            if (!Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
                return default;

            var state = new ThreatHudState();

            if (anchor.Has<Projected<ThreatState>>())
                state.ThreatPhase = ClientProjection.Read<ThreatState>(anchor).Phase;

            return state;
        }
    }
}
