using Code.EcsUi.Mvc;
using StaticMlp.Networking;

namespace StaticMlp.Features.Progression
{
    public sealed class RewardResultPopupController
        : ControllerBase<RewardResultPopupView>, IResourcePresentationController<RewardResultPopupState>
    {
        public RewardResultPopupController(
            ViewFactoryMethod<RewardResultPopupView> viewFactory,
            ControllerResourceBridgeSystem<RewardResultPopupController, RewardResultPopupState> bridge)
            : base(viewFactory)
        {
            AddModule(new BridgeSystemBinding<ControllerResourceBridgeSystem<RewardResultPopupController, RewardResultPopupState>, RewardResultPopupController>(this, bridge));
        }

        public override ViewLayer Layer => ViewLayer.Popup;

        public static string DescribeReward(RewardPackageId rewardPackageId)
        {
            if (rewardPackageId == RewardPackageCatalog.RecoveredWarCacheId)
                return "Recovered War Cache";

            return $"Reward {rewardPackageId.Value}";
        }

        public void Apply(in RewardResultPopupState state)
        {
            View.Render(in state);
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
            ref var state = ref CW.GetResource<RewardResultPopupState>();
            state.IsVisible = false;
            state.LastPresentedRewardsMask = state.CurrentAppliedRewardsMask;
        }
    }
}
