using System;
using Aspid.MVVM;
using StaticMlp.Networking;

namespace StaticMlp.Features.Progression
{
    [ViewModel]
    public sealed partial class RewardResultPopupViewModel
    {
        [OneWayBind] private RewardResultPopupViewData _data;

        public event Action Changed;

        public static string DescribeReward(RewardPackageId rewardPackageId)
        {
            if (rewardPackageId == RewardPackageCatalog.RecoveredWarCacheId)
                return "Recovered War Cache";

            return $"Reward {rewardPackageId.Value}";
        }

        public void Apply(in RewardResultPopupViewData data)
        {
            Data = data;
        }

        partial void OnDataChanged(RewardResultPopupViewData newValue)
        {
            Changed?.Invoke();
        }

        public void ClosePopup()
        {
            CW.SendEvent(new RewardResultPopupCloseIntent());
        }
    }
}
