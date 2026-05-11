using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Progression
{
    public readonly struct VerticalSliceCompleteEvent : IEvent
    {
        public readonly ushort AnchorIdValue;

        public VerticalSliceCompleteEvent(SettlementAnchorId anchorId)
        {
            AnchorIdValue = anchorId.Value;
        }

        public SettlementAnchorId AnchorId => new(AnchorIdValue);
    }
}
