using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Progression
{
    public readonly struct RewardPackageDefinition
    {
        public readonly RewardPackageId Id;
        public readonly ResourceAmount[] ResourceGrants;
        public readonly ProgressFlagId[] ProgressFlagsGranted;
        public readonly byte BossPreparationTokenGrants;

        public RewardPackageDefinition(
            RewardPackageId id,
            ResourceAmount[] resourceGrants,
            ProgressFlagId[] progressFlagsGranted,
            byte bossPreparationTokenGrants)
        {
            Id = id;
            ResourceGrants = resourceGrants;
            ProgressFlagsGranted = progressFlagsGranted;
            BossPreparationTokenGrants = bossPreparationTokenGrants;
        }
    }
}
