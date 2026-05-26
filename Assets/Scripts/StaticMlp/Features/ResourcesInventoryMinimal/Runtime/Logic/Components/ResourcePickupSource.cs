using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public struct ResourcePickupSource : IComponent
    {
        public EntityGID SourcePlayer;
        public long PlacementId;
    }
}
