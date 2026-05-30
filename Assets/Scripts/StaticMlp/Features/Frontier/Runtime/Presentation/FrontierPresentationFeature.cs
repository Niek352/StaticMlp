using Aspid.StaticEcs.Windows;
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
            var windows = CW.GetResource<WindowsController<ClientCoreWT>>();

            windows.RegisterWindow<ExpeditionSelectionWindow, EcsWindowNoData, ExpeditionSelectionView>(
                EcsResourcesWindowShellViewFactory.CreateLazy<ExpeditionSelectionView>(EXPEDITION_SELECTION_VIEW_RESOURCE_PATH),
                EcsWindowLayer.Fullscreen);
            windows.RegisterViewModel<ExpeditionSelectionWindow, EcsWindowNoData, ExpeditionSelectionSlot, ExpeditionSelectionViewModel>(
                static _ => new ExpeditionSelectionViewModel(),
                EcsWindowOpenInputBindings.Ignore<ClientCoreWT, ExpeditionSelectionWindow, EcsWindowNoData, ExpeditionSelectionViewModel>);

            windows.RegisterWindow<ThreatBannerWindow, EcsWindowNoData, ThreatBannerView>(
                EcsResourcesWindowShellViewFactory.CreateLazy<ThreatBannerView>(THREAT_BANNER_VIEW_RESOURCE_PATH),
                EcsWindowLayer.Persistent,
                persistentSortOrder: 200);
            windows.RegisterViewModel<ThreatBannerWindow, EcsWindowNoData, ThreatBannerSlot, ThreatBannerViewModel>(
                static _ => new ThreatBannerViewModel(),
                EcsWindowOpenInputBindings.Ignore<ClientCoreWT, ThreatBannerWindow, EcsWindowNoData, ThreatBannerViewModel>);

            systems.Add(new ClientFrontierPresentationBootstrapSystem(), GameplaySystemOrder.ClientPresentation + 30);
            systems.Add(new ClientExpeditionSelectionIntentSystem(), GameplaySystemOrder.ClientPresentation + 38);
            systems.Add(expeditionBridge, GameplaySystemOrder.ClientPresentation + 39);
            systems.Add(new PersistentEcsWindowHostSystem<ClientCoreWT, ThreatBannerWindow>(), GameplaySystemOrder.ClientPresentation + 40);
            systems.Add(threatBridge, GameplaySystemOrder.ClientPresentation + 41);
        }
    }
}
