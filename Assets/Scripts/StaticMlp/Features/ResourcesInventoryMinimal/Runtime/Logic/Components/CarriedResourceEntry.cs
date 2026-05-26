using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public struct CarriedResourceEntry : IMultiComponent
    {
        public ResourceId Id;
        public int Amount;

        public CarriedResourceEntry(ResourceId id, int amount)
        {
            Id = id;
            Amount = amount;
        }
    }
}
