using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Progression
{
    public readonly struct ExpeditionRewardGrantedEvent : IEvent
    {
        public readonly ushort AnchorIdValue;
        public readonly ushort RewardPackageIdValue;

        public ExpeditionRewardGrantedEvent(SettlementAnchorId anchorId, RewardPackageId rewardPackageId)
        {
            AnchorIdValue = anchorId.Value;
            RewardPackageIdValue = rewardPackageId.Value;
        }

        public SettlementAnchorId AnchorId => new(AnchorIdValue);
        public RewardPackageId RewardPackageId => new(RewardPackageIdValue);
    }
}
