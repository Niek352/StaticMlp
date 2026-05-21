using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class ServerBedrollShelterBootstrapSystem : ISystem
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
            if (completed.BuildingId != BuildingCatalogData.BedrollShelterId)
                return;

            if (!completed.FinishedBuilding.TryUnpack<ServerWT>(out var finishedEntity))
                throw new InvalidOperationException(
                    $"Bedroll shelter bootstrap: finished building {completed.FinishedBuilding} cannot be unpacked in server world.");

            var definition = BuildingCatalogData.Get(completed.BuildingId);
            if ((definition.Operation.OperationCapabilities & BuildingCapabilityFlags.ProvidesRest) == 0
                || definition.Operation.WorkerSlots == 0)
                throw new InvalidOperationException("Bedroll Shelter building definition is missing rest operation slots.");

            finishedEntity.Set(new BedrollShelterState
            {
                SlotCount = definition.Operation.WorkerSlots,
                Enabled = true
            });

            ref var slots = ref finishedEntity.Add<SW.Multi<BedSlotState>>();
            slots.Clear();
            for (byte i = 0; i < definition.Operation.WorkerSlots; i++)
                slots.Add(new BedSlotState(i, BedSlotStatus.Free));
        }
    }
}
