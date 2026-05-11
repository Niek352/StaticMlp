using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Progression
{
    public struct RewardResultPopupState : IResource
    {
        public bool IsVisible;
        public uint LastPresentedRewardsMask;
        public uint CurrentAppliedRewardsMask;
        public RewardPackageId RewardPackageId;
        public int GrantedWood;
        public int GrantedStone;
        public bool GrantsRecoveredWarCacheFlag;
        public bool ThreatRaised;
    }
}
