using Aspid.StaticEcs;
using Aspid.StaticEcs.Windows;
using FFS.Libraries.StaticEcs;
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
            var registry = CW.GetResource<EcsLinkRegistry<ClientCoreWT>>();
            registry.RegisterComponent<ExpeditionSelectionViewModel, ExpeditionSelectionViewData>(
                static (viewModel, in data) => viewModel.Apply(in data));
            registry.RegisterComponent<ThreatBannerViewModel, ThreatBannerViewData>(
                static (viewModel, in data) => viewModel.Apply(in data));

            windows.RegisterWindow<ExpeditionSelectionWindow, EcsWindowNoData, ExpeditionSelectionView>(
                EcsResourcesWindowShellViewFactory.CreateLazy<ExpeditionSelectionView>(EXPEDITION_SELECTION_VIEW_RESOURCE_PATH),
                EcsWindowLayer.Fullscreen);
            windows.RegisterLinkedViewModel<ExpeditionSelectionWindow, EcsWindowNoData, ExpeditionSelectionSlot, ExpeditionSelectionViewModel>(
                static _ => new ExpeditionSelectionViewModel(),
                static _ => ResolveSingletonPresentationEntity<ExpeditionSelectionViewData>(),
                EcsWindowOpenInputBindings.Ignore<ClientCoreWT, ExpeditionSelectionWindow, EcsWindowNoData, ExpeditionSelectionViewModel>);

            windows.RegisterWindow<ThreatBannerWindow, EcsWindowNoData, ThreatBannerView>(
                EcsResourcesWindowShellViewFactory.CreateLazy<ThreatBannerView>(THREAT_BANNER_VIEW_RESOURCE_PATH),
                EcsWindowLayer.Persistent,
                persistentSortOrder: 200);
            windows.RegisterLinkedViewModel<ThreatBannerWindow, EcsWindowNoData, ThreatBannerSlot, ThreatBannerViewModel>(
                static _ => new ThreatBannerViewModel(),
                static _ => ResolveSingletonPresentationEntity<ThreatBannerViewData>(),
                EcsWindowOpenInputBindings.Ignore<ClientCoreWT, ThreatBannerWindow, EcsWindowNoData, ThreatBannerViewModel>);

            systems.Add(new ClientFrontierPresentationBootstrapSystem(), GameplaySystemOrder.ClientPresentation + 30);
            systems.Add(new ClientExpeditionSelectionIntentSystem(), GameplaySystemOrder.ClientPresentation + 38);
            systems.Add(expeditionBridge, GameplaySystemOrder.ClientPresentation + 39);
            systems.Add(new PersistentEcsWindowHostSystem<ClientCoreWT, ThreatBannerWindow>(), GameplaySystemOrder.ClientPresentation + 40);
            systems.Add(threatBridge, GameplaySystemOrder.ClientPresentation + 41);
        }

        private static EntityGID ResolveSingletonPresentationEntity<TComponent>()
            where TComponent : struct, IComponent
        {
            foreach (var entity in CW.Query<All<TComponent>>().Entities())
                return entity.GID;

            throw new System.InvalidOperationException($"{typeof(TComponent).FullName} entity is missing.");
        }
    }
}
