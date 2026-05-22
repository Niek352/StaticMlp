using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Progression
{
    public struct RewardResultPopupSession : IResource
    {
        public bool IsVisible;
        public uint LastPresentedRewardsMask;
        public uint CurrentAppliedRewardsMask;
    }
}
