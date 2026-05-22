using Code.EcsUi.Mvc;
using StaticMlp.Networking;

namespace StaticMlp.Features.Progression
{
    public sealed class RewardResultPopupController
        : ControllerBase<RewardResultPopupView>
    {
        private RewardResultPopupViewData _lastViewData;

        public RewardResultPopupController(
            ViewFactoryMethod<RewardResultPopupView> viewFactory,
            RewardResultPopupBridgeSystem bridge)
            : base(viewFactory)
        {
            AddModule(new BridgeSystemBinding<RewardResultPopupBridgeSystem, RewardResultPopupController>(this, bridge));
        }

        public override ViewLayer Layer => ViewLayer.Popup;

        public static string DescribeReward(RewardPackageId rewardPackageId)
        {
            if (rewardPackageId == RewardPackageCatalog.RecoveredWarCacheId)
                return "Recovered War Cache";

            return $"Reward {rewardPackageId.Value}";
        }

        public void Apply(in RewardResultPopupViewData data)
        {
            _lastViewData = data;
            View.Render(in data);
        }

        protected override void OnViewInstantiated()
        {
            View.Bind(ClosePopup);
        }

        public override void Dispose()
        {
            if (View != null)
                View.Unbind();

            base.Dispose();
        }

        private static void ClosePopup()
        {
            ref var session = ref CW.GetResource<RewardResultPopupSession>();
            session.IsVisible = false;
            session.LastPresentedRewardsMask = session.CurrentAppliedRewardsMask;
        }
    }
}
