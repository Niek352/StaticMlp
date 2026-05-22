using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Frontier
{
    public static class BossHudPresentation
    {
        public static BossHudState Build()
        {
            if (!Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
                return default;

            var state = new BossHudState();

            if (anchor.Has<Projected<BossEncounterState>>())
                state.EncounterStatus = ClientProjection.Read<BossEncounterState>(anchor).Status;

            return state;
        }
    }
}
