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
            var expeditionBridge = new ExpeditionSelectionBridgeSystem();
            var threatBridge = new ThreatBannerBridgeSystem();

            systems.Add(new ClientFrontierPresentationBootstrapSystem(), GameplaySystemOrder.ClientPresentation + 30);
            systems.Add(new ControllerRegistrationSystem<ExpeditionSelectionView, ExpeditionSelectionController>(
                new ExpeditionSelectionController(ResourcesViewFactory.CreateLazy<ExpeditionSelectionView>(EXPEDITION_SELECTION_VIEW_RESOURCE_PATH), expeditionBridge)), GameplaySystemOrder.ClientPresentation + 38);
            systems.Add(expeditionBridge, GameplaySystemOrder.ClientPresentation + 39);
            systems.Add(new PersistentControllerHostSystem<ThreatBannerView, ThreatBannerController>(
                _ => new ThreatBannerController(ResourcesViewFactory.CreateLazy<ThreatBannerView>(THREAT_BANNER_VIEW_RESOURCE_PATH), threatBridge)), GameplaySystemOrder.ClientPresentation + 40);
            systems.Add(threatBridge, GameplaySystemOrder.ClientPresentation + 41);
        }
    }
}
