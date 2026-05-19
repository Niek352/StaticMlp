using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.AiNavigation
{
    public struct SpawnSourceReachabilityRequest : IComponent
    {
        public int CellId;
        public int NavVersion;
        public float RequestedAtTime;
        public float ApproxPathCost;
    }
}
