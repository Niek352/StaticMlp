using Aspid.StaticEcs.Windows;
using StaticMlp.Networking;

namespace StaticMlp.Features.Progression
{
    public sealed class RewardResultPopupViewModel : EcsWindowViewModelBase
    {
        public RewardResultPopupViewData Data { get; private set; }

        public static string DescribeReward(RewardPackageId rewardPackageId)
        {
            if (rewardPackageId == RewardPackageCatalog.RecoveredWarCacheId)
                return "Recovered War Cache";

            return $"Reward {rewardPackageId.Value}";
        }

        public void Sync(in RewardResultPopupViewData data)
        {
            Data = data;
            NotifyChanged();
        }

        public void ClosePopup()
        {
            CW.SendEvent(new RewardResultPopupCloseIntent());
        }
    }
}
