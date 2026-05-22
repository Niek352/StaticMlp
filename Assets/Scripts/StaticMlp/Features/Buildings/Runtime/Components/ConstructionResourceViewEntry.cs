using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Buildings
{
    public struct ConstructionResourceViewEntry
    {
        public ResourceId Id;
        public int Required;
        public int Delivered;

        public ConstructionResourceViewEntry(ResourceId id, int required, int delivered)
        {
            Id = id;
            Required = required;
            Delivered = delivered;
        }
    }
}
