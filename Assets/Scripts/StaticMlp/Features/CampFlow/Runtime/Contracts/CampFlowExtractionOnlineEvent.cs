using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.CampFlow
{
    public readonly struct CampFlowExtractionOnlineEvent : IEvent
    {
        private readonly ushort _anchorIdValue;

        public CampFlowExtractionOnlineEvent(SettlementAnchorId anchorId)
        {
            _anchorIdValue = anchorId.Value;
        }

        public SettlementAnchorId AnchorId => new(_anchorIdValue);
    }
}
