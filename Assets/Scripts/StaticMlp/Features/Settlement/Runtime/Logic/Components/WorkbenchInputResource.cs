using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public struct WorkbenchInputResource : IMultiComponent
    {
        public ResourceId Id;
        public int Amount;

        public WorkbenchInputResource(ResourceId id, int amount)
        {
            Id = id;
            Amount = amount;
        }
    }
}
