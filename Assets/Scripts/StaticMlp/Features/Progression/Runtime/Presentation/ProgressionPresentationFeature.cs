using Code.EcsUi.Mvc;
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

            systems.Add(new ClientProgressionPresentationBootstrapSystem(), GameplaySystemOrder.ClientPresentation + 40);
            systems.Add(new ClientRewardResultPopupStateSystem(), GameplaySystemOrder.ClientPresentation + 41);
            systems.Add(new PopupControllerHostSystem<RewardResultPopupView, RewardResultPopupController>(
                _ => new RewardResultPopupController(ResourcesViewFactory.CreateLazy<RewardResultPopupView>(REWARD_RESULT_POPUP_VIEW_RESOURCE_PATH), bridge),
                () => CW.GetResource<RewardResultPopupSession>().IsVisible), GameplaySystemOrder.ClientPresentation + 44);
            systems.Add(bridge, GameplaySystemOrder.ClientPresentation + 45);
        }
    }
}
