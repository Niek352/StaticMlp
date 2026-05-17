using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.OpenWorldResources
{
    public struct TryHarvestOpenWorldResourceCommand : IEvent
    {
        public long PlacementId;
        public ushort ToolId;
        public int HitPointXQ;
        public int HitPointYQ;
        public int HitPointZQ;
    }
}
