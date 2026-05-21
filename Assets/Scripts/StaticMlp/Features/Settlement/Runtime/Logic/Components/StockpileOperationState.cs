using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    /// <summary>
    /// Server-only component placed on a finished stockpile building entity.
    /// Records the storage capacity this individual building contributes to the settlement aggregate.
    /// </summary>
    public struct StockpileOperationState : IComponent
    {
        public ushort ContributedCapacity;
    }
}
