using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Frontier
{
    public sealed class ClientBossHudStateSystem : ISystem
    {
        public void Update()
        {
            if (!Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
                return;

            var next = new BossHudState();

            if (anchor.Has<Projected<BossEncounterState>>())
                next.EncounterStatus = ClientProjection.Read<BossEncounterState>(anchor).Status;

            CW.SetResource(next);
        }
    }
}
