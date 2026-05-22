using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Frontier
{
    public static class RaidHudPresentation
    {
        public static RaidHudState Build()
        {
            if (!Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor))
                return default;

            var state = new RaidHudState();

            if (anchor.Has<Projected<RaidScheduleState>>())
            {
                ref readonly var raid = ref ClientProjection.Read<RaidScheduleState>(anchor);
                state.Status = raid.Status;
                state.ActivateAtTick = raid.ActivateAtTick;
            }

            return state;
        }
    }
}
