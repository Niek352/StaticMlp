using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class ServerExtractionBootstrapSystem : ISystem
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
            foreach (var completed in _completedEvents)
                Handle(in completed.Value);
        }

        private static void Handle(in BuildingConstructionCompletedEvent completed)
        {
            if (!ExtractionRules.TryGetOutputResource(completed.BuildingId, out var outputResource))
                return;

            if (!completed.FinishedBuilding.TryUnpack<ServerWT>(out var finishedEntity))
                throw new InvalidOperationException(
                    $"Extraction bootstrap: finished building {completed.FinishedBuilding} cannot be unpacked in server world.");

            var definition = BuildingCatalogData.Get(completed.BuildingId);
            if ((definition.Operation.OperationCapabilities & BuildingCapabilityFlags.ExtractsFromNode) == 0
                || definition.Operation.StorageCapacity == 0
                || definition.Operation.WorkerSlots == 0)
                throw new InvalidOperationException("Extraction building definition is missing extraction operation data.");

            finishedEntity.Set(new ExtractionOperationState
            {
                OutputResourceId = outputResource.Value,
                OutputBufferAmount = 0,
                OutputBufferCapacity = definition.Operation.StorageCapacity,
                Enabled = true,
                WorkerSlotCount = definition.Operation.WorkerSlots
            });
        }
    }
}
