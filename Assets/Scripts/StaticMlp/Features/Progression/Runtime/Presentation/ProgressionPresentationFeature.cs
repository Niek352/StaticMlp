using Aspid.StaticEcs.Windows;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;

namespace StaticMlp.Features.Progression
{
    public sealed class ProgressionPresentationFeature : GameplayFeature
    {
        private const string REWARD_RESULT_POPUP_VIEW_RESOURCE_PATH = "Views/Stage1/RewardResultPopupView";

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            var bridge = new RewardResultPopupBridgeSystem();
            var windows = CW.GetResource<WindowsController<ClientCoreWT>>();

            windows.RegisterWindow<RewardResultPopupWindow, EcsWindowNoData, RewardResultPopupView>(
                EcsResourcesWindowShellViewFactory.CreateLazy<RewardResultPopupView>(REWARD_RESULT_POPUP_VIEW_RESOURCE_PATH),
                EcsWindowLayer.Popup);
            windows.RegisterViewModel<RewardResultPopupWindow, EcsWindowNoData, RewardResultPopupSlot, RewardResultPopupViewModel>(
                static _ => new RewardResultPopupViewModel(),
                EcsWindowOpenInputBindings.Ignore<ClientCoreWT, RewardResultPopupWindow, EcsWindowNoData, RewardResultPopupViewModel>);

            systems.Add(new ClientProgressionPresentationBootstrapSystem(), GameplaySystemOrder.ClientPresentation + 40);
            systems.Add(new ClientRewardResultPopupStateSystem(), GameplaySystemOrder.ClientPresentation + 41);
            systems.Add(new ClientRewardResultPopupIntentSystem(), GameplaySystemOrder.ClientPresentation + 43);
            systems.Add(new StateDrivenEcsWindowHostSystem<ClientCoreWT, RewardResultPopupWindow, RewardResultPopupSession>(
                static (in RewardResultPopupSession session) => session.IsVisible), GameplaySystemOrder.ClientPresentation + 44);
            systems.Add(bridge, GameplaySystemOrder.ClientPresentation + 45);
        }
    }
}
