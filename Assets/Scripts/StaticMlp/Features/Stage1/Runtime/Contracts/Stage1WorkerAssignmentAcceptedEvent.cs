using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public readonly struct Stage1WorkerAssignmentAcceptedEvent : IEvent
    {
        private readonly ushort _anchorIdValue;

        public Stage1WorkerAssignmentAcceptedEvent(SettlementAnchorId anchorId)
        {
            _anchorIdValue = anchorId.Value;
        }

        public SettlementAnchorId AnchorId => new(_anchorIdValue);
    }
}
