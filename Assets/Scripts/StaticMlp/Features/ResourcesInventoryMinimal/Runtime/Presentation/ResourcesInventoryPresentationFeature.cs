using Aspid.StaticEcs;
using Aspid.StaticEcs.Windows;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public sealed class ResourcesInventoryPresentationFeature : GameplayFeature
    {
        private const string RESOURCE_PICKUP_VIEW_PATH = "Views/ResourcesInventoryMinimal/ResourcePickupView";
        private const string INVENTORY_HUD_VIEW_PATH = "Views/ResourcesInventoryMinimal/ResourcesInventoryHudView";

        public override void RegisterPrefabs()
        {
            NetArchetypeRegistry.RegisterClient(ResourcesInventoryMinimalGameplayFeature.RESOURCE_PICKUP, e =>
            {
                e.Set(new ViewPath(RESOURCE_PICKUP_VIEW_PATH));
                e.Set(new ViewTransform
                {
                    RenderRotation = Quaternion.identity
                });
                e.Set(new ResourcePickupViewState());
            });
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            var hudBridge = new ResourcesInventoryHudBridgeSystem();
            var windows = CW.GetResource<WindowsController<ClientCoreWT>>();
            var registry = CW.GetResource<EcsLinkRegistry<ClientCoreWT>>();
            registry.RegisterComponent<ResourcesInventoryHudViewModel, ResourcesInventoryHudViewData>(
                binding: Binding);

            windows.RegisterWindow<ResourcesInventoryHudWindow, EcsWindowNoData, ResourcesInventoryHudShellView>(
                EcsResourcesWindowShellViewFactory.CreateLazy<ResourcesInventoryHudShellView>(INVENTORY_HUD_VIEW_PATH),
                EcsWindowLayer.Persistent,
                persistentSortOrder: 20);
            windows.RegisterLinkedViewModel<ResourcesInventoryHudWindow, EcsWindowNoData, ResourcesInventoryHudSlot, ResourcesInventoryHudViewModel>(
                static _ => new ResourcesInventoryHudViewModel(),
                static _ => ResolveSingletonPresentationEntity<ResourcesInventoryHudViewData>(),
                EcsWindowOpenInputBindings.Ignore<ClientCoreWT, ResourcesInventoryHudWindow, EcsWindowNoData, ResourcesInventoryHudViewModel>);

            CW.SetResource(ResourcesInventoryConfig.CreateDefault());
            systems.Add(new ClientResourcePickupViewBindSystem(), ViewSystemOrder.BindViews - 10);
            systems.Add(new ClientResourcePickupMagnetViewSystem(), ViewSystemOrder.BuildPresentationState);
            systems.Add(new ClientResourcesInventoryPresentationBootstrapSystem(), GameplaySystemOrder.ClientPresentation + 29);
            systems.Add(new PersistentEcsWindowHostSystem<ClientCoreWT, ResourcesInventoryHudWindow>(), GameplaySystemOrder.ClientPresentation + 30);
            systems.Add(hudBridge, GameplaySystemOrder.ClientPresentation + 31);
        }

        private static void Binding(ResourcesInventoryHudViewModel viewModel, in ResourcesInventoryHudViewData data)
        {
            viewModel.Apply(in data);
        }

        public override void RegisterClientViewSync(ViewSyncBuilder views)
        {
            views.Register<ResourcePickupViewState>();
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
