using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public struct WorkbenchOutputResource : IMultiComponent
    {
        public ResourceId Id;
        public int Amount;

        public WorkbenchOutputResource(ResourceId id, int amount)
        {
            Id = id;
            Amount = amount;
        }
    }
}
