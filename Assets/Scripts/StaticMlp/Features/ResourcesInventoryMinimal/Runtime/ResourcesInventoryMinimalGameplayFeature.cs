using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public sealed class ResourcesInventoryMinimalGameplayFeature : GameplayFeature
    {
        public const ushort RESOURCE_PICKUP = 410;

        public override void RegisterPrefabs()
        {
            NetArchetypeRegistry.RegisterClient(RESOURCE_PICKUP, e =>
            {
                e.Set<ResourcePickupTag>();
            });

            NetArchetypeRegistry.RegisterServer(RESOURCE_PICKUP, e =>
            {
                e.Set<ResourcePickupTag>();
            });
        }

        public override void RegisterServerResources()
        {
            SW.SetResource(ResourcesInventoryConfig.CreateDefault());
            SW.SetResource(new ResourcePickupFactory());
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerResourcesInventorySeedSystem(), GameplaySystemOrder.ServerConnectionGameplay + 20);
            systems.Add(new ServerResourcePickupSpawnSystem(), GameplaySystemOrder.Gameplay - 32);
            systems.Add(new ServerResourcePickupCollectSystem(), GameplaySystemOrder.Gameplay - 30);
            systems.Add(new ServerResourcePickupCleanupSystem(), GameplaySystemOrder.Gameplay - 29);
        }
    }
}
