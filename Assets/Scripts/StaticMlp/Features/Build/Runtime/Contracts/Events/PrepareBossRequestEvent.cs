using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Build
{
    public readonly struct PrepareBossRequestEvent : IEvent
    {
        public readonly ushort AnchorIdValue;

        public PrepareBossRequestEvent(SettlementAnchorId anchorId)
        {
            AnchorIdValue = anchorId.Value;
        }

        public PrepareBossRequestEvent(ushort anchorIdValue)
        {
            AnchorIdValue = anchorIdValue;
        }

        public SettlementAnchorId AnchorId => new(AnchorIdValue);
    }
}
