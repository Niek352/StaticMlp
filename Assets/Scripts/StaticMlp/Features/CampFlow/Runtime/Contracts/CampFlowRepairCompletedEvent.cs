using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.CampFlow
{
    public readonly struct CampFlowRepairCompletedEvent : IEvent
    {
        public readonly ushort AnchorIdValue;

        public CampFlowRepairCompletedEvent(SettlementAnchorId anchorId)
        {
            AnchorIdValue = anchorId.Value;
        }

        public SettlementAnchorId AnchorId => new(AnchorIdValue);
    }
}
