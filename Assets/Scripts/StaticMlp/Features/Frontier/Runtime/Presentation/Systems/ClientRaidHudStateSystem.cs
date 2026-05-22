using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Frontier
{
    public sealed class ClientRaidHudStateSystem : ISystem
    {
        public void Update()
        {
            if (!Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
                return;

            var next = new RaidHudState();

            if (anchor.Has<Projected<RaidScheduleState>>())
            {
                ref readonly var raid = ref ClientProjection.Read<RaidScheduleState>(anchor);
                next.Status = raid.Status;
                next.ActivateAtTick = raid.ActivateAtTick;
            }

            CW.SetResource(next);
        }
    }
}
