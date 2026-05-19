using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.AiNavigation
{
    public struct NavWorkBudgetCounter : IComponent
    {
        public int PathRequestsThisTick;
        public int ReachabilityChecksThisTick;
        public int RebuildRequestsQueuedThisTick;
        public int RebuildsStartedThisTick;
    }
}
