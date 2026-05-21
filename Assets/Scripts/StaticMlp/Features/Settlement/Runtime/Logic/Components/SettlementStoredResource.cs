using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public struct SettlementStoredResource : IMultiComponent
    {
        public ResourceId Id;
        public int Amount;

        public SettlementStoredResource(ResourceId id, int amount)
        {
            Id = id;
            Amount = amount;
        }
    }
}
