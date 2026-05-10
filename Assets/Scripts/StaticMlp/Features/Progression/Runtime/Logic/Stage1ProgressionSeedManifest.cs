using System;
using StaticMlp.Features.Build;

namespace StaticMlp.Features.Progression
{
    public static class Stage1ProgressionSeedManifest
    {
        private static readonly ProgressFlagId[] STARTING_FLAGS = Array.Empty<ProgressFlagId>();

        private static readonly BuildModuleId[] STARTING_UNLOCKED_MODULES =
        {
            BuildModuleCatalog.PoisonArrowModuleId,
            BuildModuleCatalog.FireFlaskModuleId
        };

        private static readonly RewardPackageId[] STARTING_AVAILABLE_REWARDS = Array.Empty<RewardPackageId>();

        public static Stage1ProgressionSeed CreateResource()
        {
            return new Stage1ProgressionSeed(
                STARTING_FLAGS,
                STARTING_UNLOCKED_MODULES,
                STARTING_AVAILABLE_REWARDS);
        }
    }
}
