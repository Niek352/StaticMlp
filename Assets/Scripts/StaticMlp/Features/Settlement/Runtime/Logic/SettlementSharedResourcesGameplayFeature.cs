using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementSharedResourcesGameplayFeature : GameplayFeature
    {
        public override void RegisterNetworkEvents()
        {
            ProjectionRegistry.Register<SettlementSharedResources>();
        }

        public override void RegisterPrefabs()
        {
            NetArchetypeRegistry.RegisterClient(SettlementNetworkArchetypeIds.ResourceStorage, e =>
            {
                e.Set<SettlementResourceStorageTag>();
            });

            NetArchetypeRegistry.RegisterServer(SettlementNetworkArchetypeIds.ResourceStorage, e =>
            {
                e.Set<SettlementResourceStorageTag>();
            });
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerSettlementSharedResourcesSpawnSystem(), GameplaySystemOrder.ServerConnectionGameplay + 5);
        }
    }
}
