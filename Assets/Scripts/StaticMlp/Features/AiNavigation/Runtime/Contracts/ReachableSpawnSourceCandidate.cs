using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.AiNavigation
{
    public struct ReachableSpawnSourceCandidate : IComponent
    {
        public EntityGID Source;
        public int ZoneId;
        public int NavVersion;
        public float ApproxPathCost;
    }
}
