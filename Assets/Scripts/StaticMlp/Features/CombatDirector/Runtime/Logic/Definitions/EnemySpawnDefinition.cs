namespace StaticMlp.Features.CombatDirector
{
    public readonly struct EnemySpawnDefinition
    {
        public readonly EnemyRole Role;
        public readonly float BudgetCost;
        public readonly int MinCountPerWave;
        public readonly int MaxCountPerWave;

        public EnemySpawnDefinition(
            EnemyRole role,
            float budgetCost,
            int minCountPerWave,
            int maxCountPerWave)
        {
            Role = role;
            BudgetCost = budgetCost;
            MinCountPerWave = minCountPerWave;
            MaxCountPerWave = maxCountPerWave;
        }
    }
}
