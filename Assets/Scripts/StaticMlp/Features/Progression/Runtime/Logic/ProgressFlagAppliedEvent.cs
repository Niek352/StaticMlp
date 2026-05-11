using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Progression
{
    public readonly struct ProgressFlagAppliedEvent : IEvent
    {
        public readonly ushort AnchorIdValue;
        public readonly ushort FlagIdValue;

        public ProgressFlagAppliedEvent(SettlementAnchorId anchorId, ProgressFlagId flagId)
        {
            AnchorIdValue = anchorId.Value;
            FlagIdValue = flagId.Value;
        }

        public SettlementAnchorId AnchorId => new(AnchorIdValue);
        public ProgressFlagId FlagId => new(FlagIdValue);
    }
}
