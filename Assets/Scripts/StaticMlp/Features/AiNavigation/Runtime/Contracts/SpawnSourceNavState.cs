using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.AiNavigation
{
    public struct SpawnSourceNavState : IComponent
    {
        public SpawnSourceNavStatus Status;
        public float LastCheckedTime;
        public float NextCheckTime;
        public float ApproxPathCost;
        public int NavVersion;
    }
}
