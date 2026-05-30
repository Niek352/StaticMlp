using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Progression
{
    public struct RewardResultPopupViewData : IComponent, ITrackableAdded, ITrackableChanged
    {
        public RewardPackageId RewardPackageId;
        public int GrantedWood;
        public int GrantedStone;
        public bool GrantsRecoveredWarCacheFlag;
        public bool ThreatRaised;
    }
}
