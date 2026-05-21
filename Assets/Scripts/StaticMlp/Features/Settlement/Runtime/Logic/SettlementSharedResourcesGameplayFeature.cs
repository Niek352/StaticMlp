using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementSharedResourcesGameplayFeature : GameplayFeature
    {
        public override void RegisterNetworkEvents()
        {
            ProjectionRegistry.Register<SettlementSharedResources>();
            ProjectionRegistry.RegisterMulti<SettlementStoredResource>();
            ProjectionRegistry.Register<Stage1SettlementProgression>();
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

        public override void RegisterServerResources()
        {
            ResourceCatalogValidator.Validate(ResourceCatalog.All);
            SW.SetResource(new SettlementSharedResourcesFactory());
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerSettlementSharedResourcesSpawnSystem(), (short)(GameplaySystemOrder.ServerConnectionGameplay - 15));
            systems.Add(new ServerDepositConstructionResourcesSystem(), GameplaySystemOrder.Gameplay - 39);
            systems.Add(new ServerApplyConstructionBuildWorkSystem(), GameplaySystemOrder.Gameplay - 39);
            systems.Add(new ServerStockpileOperationBootstrapSystem(), GameplaySystemOrder.Gameplay - 48);
        }
    }
}
