using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    /// <summary>
    /// Settlement-owned event sent when a completed building should contribute storage capacity.
    /// Consumed by ServerStockpileOperationBootstrapSystem to attach StockpileOperationState
    /// and increase the settlement aggregate capacity.
    /// </summary>
    public readonly struct StockpileCapacityContributionEvent : IEvent
    {
        public readonly EntityGID FinishedBuilding;
        public readonly ushort ContributedCapacity;

        public StockpileCapacityContributionEvent(EntityGID finishedBuilding, ushort contributedCapacity)
        {
            FinishedBuilding = finishedBuilding;
            ContributedCapacity = contributedCapacity;
        }
    }
}
