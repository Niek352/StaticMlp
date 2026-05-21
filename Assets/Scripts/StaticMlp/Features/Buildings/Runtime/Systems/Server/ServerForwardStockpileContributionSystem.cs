using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    /// <summary>
    /// Server system that forwards building completion events for stockpile buildings
    /// into Settlement-owned StockpileCapacityContributionEvent.
    /// This system does not know stockpile internals; it only translates the generic
    /// completion event into a Settlement-owned contribution fact.
    /// </summary>
    public sealed class ServerForwardStockpileContributionSystem : ISystem
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
            var definition = BuildingCatalogData.Get(completed.BuildingId);
            if ((definition.Operation.OperationCapabilities & BuildingCapabilityFlags.ProvidesStorage) == 0)
                return;

            SW.SendEvent(new StockpileCapacityContributionEvent(
                completed.FinishedBuilding,
                definition.Operation.StorageCapacity));
        }
    }
}
