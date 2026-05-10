using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Progression
{
    public readonly struct RewardPackageDefinition
    {
        public readonly RewardPackageId Id;
        public readonly ResourceAmount[] ResourceGrants;
        public readonly ProgressFlagId[] ProgressFlagsGranted;

        public RewardPackageDefinition(
            RewardPackageId id,
            ResourceAmount[] resourceGrants,
            ProgressFlagId[] progressFlagsGranted)
        {
            Id = id;
            ResourceGrants = resourceGrants;
            ProgressFlagsGranted = progressFlagsGranted;
        }
    }
}
