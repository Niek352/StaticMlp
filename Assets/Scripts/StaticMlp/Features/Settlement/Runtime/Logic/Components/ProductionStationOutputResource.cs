using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public struct ProductionStationOutputResource : IMultiComponent
    {
        public ResourceId Id;
        public int Amount;

        public ProductionStationOutputResource(ResourceId id, int amount)
        {
            Id = id;
            Amount = amount;
        }
    }
}
