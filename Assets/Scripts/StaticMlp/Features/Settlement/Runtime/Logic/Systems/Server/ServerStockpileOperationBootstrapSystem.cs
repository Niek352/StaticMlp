using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    /// <summary>
    /// Server system that consumes StockpileCapacityContributionEvent (forwarded from building
    /// completion events by the Buildings feature). Attaches StockpileOperationState on the
    /// finished building entity and increases the settlement aggregate capacity.
    /// </summary>
    public sealed class ServerStockpileOperationBootstrapSystem : ISystem
    {
        private EventReceiver<ServerWT, StockpileCapacityContributionEvent> _contributions;

        public void Init()
        {
            _contributions = SW.RegisterEventReceiver<StockpileCapacityContributionEvent>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _contributions);
        }

        public void Update()
        {
            foreach (var e in _contributions)
                Handle(in e.Value);
        }

        private static void Handle(in StockpileCapacityContributionEvent contribution)
        {
            if (!contribution.FinishedBuilding.TryUnpack<ServerWT>(out var finishedEntity))
                throw new InvalidOperationException(
                    $"Stockpile bootstrap: finished building {contribution.FinishedBuilding} cannot be unpacked in server world.");

            finishedEntity.Set(new StockpileOperationState
            {
                ContributedCapacity = contribution.ContributedCapacity
            });

            var storageEntity = SettlementSharedResourcesQuery.GetServerEntity();
            ref var storage = ref ReplicationMut.Mut<SettlementSharedResources>(storageEntity);
            storage.Capacity = StockpileRules.AddCapacityContribution(storage.Capacity, contribution.ContributedCapacity);
        }
    }
}
