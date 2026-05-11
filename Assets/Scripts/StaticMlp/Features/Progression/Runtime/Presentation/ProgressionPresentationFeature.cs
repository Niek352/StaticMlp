using Code.EcsUi.Mvc;
using StaticMlp.Game.Bootstrap;

namespace StaticMlp.Features.Progression
{
    public sealed class ProgressionPresentationFeature : GameplayFeature
    {
        private const string REWARD_RESULT_POPUP_VIEW_RESOURCE_PATH = "Views/Stage1/RewardResultPopupView";

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            var bridge = new ControllerResourceBridgeSystem<RewardResultPopupController, RewardResultPopupState>((controller, state) => controller.Apply(in state));

            systems.Add(new ClientProgressionPresentationBootstrapSystem(), GameplaySystemOrder.ClientPresentation + 40);
            systems.Add(new ClientRewardResultPopupStateSystem(), GameplaySystemOrder.ClientPresentation + 41);
            systems.Add(new PopupControllerHostSystem<RewardResultPopupView, RewardResultPopupController, RewardResultPopupState>(
                _ => new RewardResultPopupController(ResourcesViewFactory.CreateLazy<RewardResultPopupView>(REWARD_RESULT_POPUP_VIEW_RESOURCE_PATH), bridge),
                state => state.IsVisible,
                (controller, state) => controller.Apply(in state)), GameplaySystemOrder.ClientPresentation + 43);
        }
    }
}
