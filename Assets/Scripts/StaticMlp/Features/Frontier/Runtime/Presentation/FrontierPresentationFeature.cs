using Code.EcsUi.Mvc;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;

namespace StaticMlp.Features.Frontier
{
    public sealed class FrontierPresentationFeature : GameplayFeature
    {
        private const string EXPEDITION_SELECTION_VIEW_RESOURCE_PATH = "Views/Stage1/ExpeditionSelectionView";

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            var expeditionBridge = new ControllerResourceBridgeSystem<ExpeditionSelectionController, ExpeditionSelectionScreenState>((controller, state) => controller.Apply(in state));

            systems.Add(new ClientFrontierPresentationBootstrapSystem(), GameplaySystemOrder.ClientPresentation + 30);
            systems.Add(new ClientThreatBannerStateSystem(), GameplaySystemOrder.ClientPresentation + 31);
            systems.Add(new ClientExpeditionSelectionScreenStateSystem(), GameplaySystemOrder.ClientPresentation + 32);
            systems.Add(new ControllerRegistrationSystem<ExpeditionSelectionView, ExpeditionSelectionController>(
                _ => new ExpeditionSelectionController(ResourcesViewFactory.CreateLazy<ExpeditionSelectionView>(EXPEDITION_SELECTION_VIEW_RESOURCE_PATH), expeditionBridge),
                controller =>
                {
                    var state = CW.GetResource<ExpeditionSelectionScreenState>();
                    controller.Apply(in state);
                }), GameplaySystemOrder.ClientPresentation + 34);
        }
    }
}
