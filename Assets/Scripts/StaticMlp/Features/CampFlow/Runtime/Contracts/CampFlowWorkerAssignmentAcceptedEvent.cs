using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.CampFlow
{
    public readonly struct CampFlowWorkerAssignmentAcceptedEvent : IEvent
    {
        private readonly ushort _anchorIdValue;

        public CampFlowWorkerAssignmentAcceptedEvent(SettlementAnchorId anchorId)
        {
            _anchorIdValue = anchorId.Value;
        }

        public SettlementAnchorId AnchorId => new(_anchorIdValue);
    }
}
