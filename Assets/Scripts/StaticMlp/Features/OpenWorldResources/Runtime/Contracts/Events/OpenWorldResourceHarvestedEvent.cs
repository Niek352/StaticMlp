using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.OpenWorldResources
{
    public struct OpenWorldResourceHarvestedEvent : IEvent
    {
        public EntityGID SourcePlayer;
        public long PlacementId;
        public ResourceAmount Resource;
        public int HitPointXQ;
        public int HitPointYQ;
        public int HitPointZQ;
        public bool WasDepleted;
    }
}
