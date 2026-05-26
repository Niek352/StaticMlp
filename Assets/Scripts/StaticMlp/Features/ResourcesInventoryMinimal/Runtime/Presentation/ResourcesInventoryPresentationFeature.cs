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
            CW.SetResource(ResourcesInventoryConfig.CreateDefault());
            systems.Add(new ClientResourcePickupMagnetViewSystem(), ViewSystemOrder.BuildPresentationState);
        }

        public override void RegisterClientViewSync(ViewSyncBuilder views)
        {
            views.Register<ResourcePickupViewState>();
        }
    }
}
