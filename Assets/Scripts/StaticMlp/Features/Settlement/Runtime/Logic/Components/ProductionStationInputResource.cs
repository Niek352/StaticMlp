using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public struct ProductionStationInputResource : IMultiComponent
    {
        public ResourceId Id;
        public int Amount;

        public ProductionStationInputResource(ResourceId id, int amount)
        {
            Id = id;
            Amount = amount;
        }
    }
}
