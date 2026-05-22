using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class ServerWorkbenchBootstrapSystem : ISystem
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
            if (completed.BuildingId != BuildingCatalogData.WorkbenchId)
                return;

            if (!completed.FinishedBuilding.TryUnpack<ServerWT>(out var finishedEntity))
                throw new InvalidOperationException(
                    $"Workbench bootstrap: finished building {completed.FinishedBuilding} cannot be unpacked in server world.");

            var definition = BuildingCatalogData.Get(completed.BuildingId);
            if ((definition.Operation.OperationCapabilities & BuildingCapabilityFlags.OpensQueue) == 0
                || definition.Operation.WorkerSlots == 0)
                throw new InvalidOperationException("Workbench building definition is missing production queue operation data.");

            finishedEntity.Set(new WorkbenchOperationState
            {
                ActiveRecipeId = WorkbenchRecipeCatalog.PlanksId.Value,
                Enabled = true,
                WorkerSlotCount = definition.Operation.WorkerSlots,
                WorkDone = 0f
            });
            WorkbenchResourceAccess.InitializeRows(finishedEntity);
        }
    }
}
