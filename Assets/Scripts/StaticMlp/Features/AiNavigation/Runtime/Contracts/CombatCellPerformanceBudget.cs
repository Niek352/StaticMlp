using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.AiNavigation
{
    public struct CombatCellPerformanceBudget : IComponent
    {
        public int MaxAliveEnemies;
        public int MaxNewSpawnsPerPeak;
        public int MaxPathRequestsPerTick;
        public int MaxReachabilityChecksPerTick;
        public int MaxAiDecisionsPerTick;
    }
}
