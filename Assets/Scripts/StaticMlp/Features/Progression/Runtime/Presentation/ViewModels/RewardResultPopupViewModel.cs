using Aspid.MVVM;
using StaticMlp.Networking;

namespace StaticMlp.Features.Progression
{
    [ViewModel]
    public sealed partial class RewardResultPopupViewModel
    {
        [OneWayBind] private string _summary;

        public void Apply(in RewardResultPopupViewData data)
        {
            Summary =
                $"Reward {data.RewardPackageId.Value}\n" +
                $"Wood: +{data.GrantedWood}\n" +
                $"Stone: +{data.GrantedStone}\n" +
                $"Recovered cache: {data.GrantsRecoveredWarCacheFlag}\n" +
                $"Threat raised: {data.ThreatRaised}";
        }

        [RelayCommand]
        private void ClosePopup()
        {
            CW.SendEvent(new RewardResultPopupCloseIntent());
        }
    }
}
