using Code.EcsUi.Mvc;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;

namespace StaticMlp.Features.Frontier
{
    public sealed class FrontierPresentationFeature : GameplayFeature
    {
        private const string EXPEDITION_SELECTION_VIEW_RESOURCE_PATH = "Views/Stage1/ExpeditionSelectionView";
        private const string THREAT_BANNER_VIEW_RESOURCE_PATH = "Views/Stage1/ThreatBannerView";

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            var expeditionBridge = new ControllerResourceBridgeSystem<ExpeditionSelectionController, ExpeditionSelectionScreenState>();
            var threatBridge = new ControllerResourceBridgeSystem<ThreatBannerController, ThreatBannerState>();

            systems.Add(new ClientFrontierPresentationBootstrapSystem(), GameplaySystemOrder.ClientPresentation + 30);
            systems.Add(new ClientThreatBannerStateSystem(), GameplaySystemOrder.ClientPresentation + 31);
            systems.Add(new ClientExpeditionSelectionScreenStateSystem(), GameplaySystemOrder.ClientPresentation + 32);
            systems.Add(new ControllerRegistrationSystem<ExpeditionSelectionView, ExpeditionSelectionController>(
                new ExpeditionSelectionController(ResourcesViewFactory.CreateLazy<ExpeditionSelectionView>(EXPEDITION_SELECTION_VIEW_RESOURCE_PATH), expeditionBridge)), GameplaySystemOrder.ClientPresentation + 34);
            systems.Add(expeditionBridge, GameplaySystemOrder.ClientPresentation + 35);
            systems.Add(new PersistentControllerHostSystem<ThreatBannerView, ThreatBannerController>(
                _ => new ThreatBannerController(ResourcesViewFactory.CreateLazy<ThreatBannerView>(THREAT_BANNER_VIEW_RESOURCE_PATH), threatBridge)), GameplaySystemOrder.ClientPresentation + 36);
            systems.Add(threatBridge, GameplaySystemOrder.ClientPresentation + 37);
        }
    }
}
