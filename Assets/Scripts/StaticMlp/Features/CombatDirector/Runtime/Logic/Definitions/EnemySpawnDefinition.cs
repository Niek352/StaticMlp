namespace StaticMlp.Features.CombatDirector
{
    public readonly struct EnemySpawnDefinition
    {
        public readonly EnemyRole Role;
        public readonly float BudgetCost;
        public readonly float MaxHealth;
        public readonly int MinCountPerWave;
        public readonly int MaxCountPerWave;

        public EnemySpawnDefinition(
            EnemyRole role,
            float budgetCost,
            float maxHealth,
            int minCountPerWave,
            int maxCountPerWave)
        {
            Role = role;
            BudgetCost = budgetCost;
            MaxHealth = maxHealth;
            MinCountPerWave = minCountPerWave;
            MaxCountPerWave = maxCountPerWave;
        }
    }
}
