using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.CampFlow
{
    public readonly struct CampFlowLoadoutPreparedEvent : IEvent
    {
        public readonly ushort AnchorIdValue;

        public CampFlowLoadoutPreparedEvent(SettlementAnchorId anchorId)
        {
            AnchorIdValue = anchorId.Value;
        }

        public SettlementAnchorId AnchorId => new(AnchorIdValue);
    }
}
