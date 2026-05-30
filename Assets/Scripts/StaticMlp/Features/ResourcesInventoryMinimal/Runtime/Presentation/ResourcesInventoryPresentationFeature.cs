using Aspid.StaticEcs.Windows;
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

            windows.RegisterWindow<ResourcesInventoryHudWindow, EcsWindowNoData, ResourcesInventoryHudView>(
                EcsResourcesWindowShellViewFactory.CreateLazy<ResourcesInventoryHudView>(INVENTORY_HUD_VIEW_PATH),
                EcsWindowLayer.Persistent,
                persistentSortOrder: 20);
            windows.RegisterViewModel<ResourcesInventoryHudWindow, EcsWindowNoData, ResourcesInventoryHudSlot, ResourcesInventoryHudViewModel>(
                static _ => new ResourcesInventoryHudViewModel(),
                EcsWindowOpenInputBindings.Ignore<ClientCoreWT, ResourcesInventoryHudWindow, EcsWindowNoData, ResourcesInventoryHudViewModel>);

            CW.SetResource(ResourcesInventoryConfig.CreateDefault());
            systems.Add(new ClientResourcePickupViewBindSystem(), ViewSystemOrder.BindViews - 10);
            systems.Add(new ClientResourcePickupMagnetViewSystem(), ViewSystemOrder.BuildPresentationState);
            systems.Add(new PersistentEcsWindowHostSystem<ClientCoreWT, ResourcesInventoryHudWindow>(), GameplaySystemOrder.ClientPresentation + 30);
            systems.Add(hudBridge, GameplaySystemOrder.ClientPresentation + 31);
        }

        public override void RegisterClientViewSync(ViewSyncBuilder views)
        {
            views.Register<ResourcePickupViewState>();
        }
    }
}
