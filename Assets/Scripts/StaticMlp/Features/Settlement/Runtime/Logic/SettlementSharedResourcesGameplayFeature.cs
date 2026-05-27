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
            ProjectionRegistry.Register<StockpileOperationState>();
            ProjectionRegistry.Register<ExtractionOperationState>();
            ProjectionRegistry.Register<WorkbenchOperationState>();
            ProjectionRegistry.Register<BedrollShelterState>();
            DepositCarriedResourcesToStockpileEventCodec.Register();
            RequestRegistry.Register<DepositCarriedResourcesToStockpileRequestEvent, DepositCarriedResourcesToStockpileResultEvent>(
                new DepositCarriedResourcesToStockpileHandler(),
                projector: null,
                serverOrder: GameplaySystemOrder.Gameplay - 41);
            RequestRegistry.Register<CollectExtractionOutputRequestEvent, CollectExtractionOutputResultEvent>(
                new CollectExtractionOutputHandler(),
                projector: null,
                serverOrder: GameplaySystemOrder.Gameplay - 40);
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
            systems.Add(new ServerBedrollShelterBootstrapSystem(), GameplaySystemOrder.Gameplay - 48);
            systems.Add(new ServerWorkbenchBootstrapSystem(), GameplaySystemOrder.Gameplay - 48);
            systems.Add(new ServerExtractionBootstrapSystem(), GameplaySystemOrder.Gameplay - 48);
            systems.Add(new ServerExtractionOperationSystem(), GameplaySystemOrder.Gameplay - 47);
            systems.Add(new ServerTransferExtractionOutputToStockpileSystem(), GameplaySystemOrder.Gameplay - 39);
        }
    }
}
