namespace StaticMlp.Features.Settlement
{
    public readonly struct WorkerRoleDefinition
    {
        public readonly WorkerRoleId Id;
        public readonly WorkerJobFlags AllowedJobs;
        public readonly float BuildSpeedMultiplier;
        public readonly float ProductionSpeedMultiplier;
        public readonly int HousingCost;
        public readonly int RaidCombatValue;

        public WorkerRoleDefinition(
            WorkerRoleId id,
            WorkerJobFlags allowedJobs,
            float buildSpeedMultiplier,
            float productionSpeedMultiplier,
            int housingCost,
            int raidCombatValue)
        {
            Id = id;
            AllowedJobs = allowedJobs;
            BuildSpeedMultiplier = buildSpeedMultiplier;
            ProductionSpeedMultiplier = productionSpeedMultiplier;
            HousingCost = housingCost;
            RaidCombatValue = raidCombatValue;
        }
    }
}
