namespace StaticMlp.Features.AiNavigation
{
    public static class CombatCellPerformanceBudgetDefaults
    {
        public static CombatCellPerformanceBudget Create()
        {
            return new CombatCellPerformanceBudget
            {
                MaxAliveEnemies = 32,
                MaxNewSpawnsPerPeak = 8,
                MaxPathRequestsPerTick = 4,
                MaxReachabilityChecksPerTick = 2,
                MaxAiDecisionsPerTick = 16,
                MaxNavRebuildStartsPerTick = 1
            };
        }
    }
}
