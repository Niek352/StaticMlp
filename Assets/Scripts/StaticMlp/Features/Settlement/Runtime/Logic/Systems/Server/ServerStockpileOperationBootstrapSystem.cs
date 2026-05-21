using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    public sealed class ServerStockpileOperationBootstrapSystem : ISystem
    {
        private EventReceiver<ServerWT, BuildingConstructionCompletedEvent> _completedEvents;

        public void Init()
        {
            _completedEvents = SW.RegisterEventReceiver<BuildingConstructionCompletedEvent>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _completedEvents);
        }

        public void Update()
        {
            foreach (var e in _completedEvents)
                Handle(in e.Value);
        }

        private static void Handle(in BuildingConstructionCompletedEvent completed)
        {
            if (completed.BuildingId != BuildingCatalogData.StockpileId)
                return;

            if (!completed.FinishedBuilding.TryUnpack<ServerWT>(out var finishedEntity))
                throw new InvalidOperationException(
                    $"Stockpile bootstrap: finished building {completed.FinishedBuilding} cannot be unpacked in server world.");

            var definition = BuildingCatalogData.Get(completed.BuildingId);
            if ((definition.Operation.OperationCapabilities & BuildingCapabilityFlags.ProvidesStorage) == 0
                || definition.Operation.StorageCapacity == 0)
                throw new InvalidOperationException("Stockpile building definition is missing storage operation capacity.");

            finishedEntity.Set(new StockpileOperationState
            {
                ContributedCapacity = definition.Operation.StorageCapacity,
                Enabled = true
            });

            var storageEntity = SettlementSharedResourcesQuery.GetServerEntity();
            ref var storage = ref ReplicationMut.Mut<SettlementSharedResources>(storageEntity);
            storage.Capacity = StockpileRules.AddCapacityContribution(storage.Capacity, definition.Operation.StorageCapacity);
        }
    }
}
