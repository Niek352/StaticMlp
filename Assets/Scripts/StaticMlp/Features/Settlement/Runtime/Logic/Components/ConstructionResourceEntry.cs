using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public struct ConstructionResourceEntry : IMultiComponent
    {
        public ResourceId Id;
        public int Required;
        public int Delivered;

        public ConstructionResourceEntry(ResourceId id, int required, int delivered = 0)
        {
            Id = id;
            Required = required;
            Delivered = delivered;
        }
    }
}
