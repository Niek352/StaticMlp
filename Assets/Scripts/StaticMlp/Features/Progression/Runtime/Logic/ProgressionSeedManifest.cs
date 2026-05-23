using System;
using StaticMlp.Features.Loadout;

namespace StaticMlp.Features.Progression
{
    public static class ProgressionSeedManifest
    {
        private static readonly ProgressFlagId[] STARTING_FLAGS = Array.Empty<ProgressFlagId>();

        private static readonly LoadoutModuleId[] STARTING_UNLOCKED_MODULES =
        {
            LoadoutModuleCatalog.PoisonArrowModuleId,
            LoadoutModuleCatalog.FireFlaskModuleId
        };

        private static readonly RewardPackageId[] STARTING_AVAILABLE_REWARDS = Array.Empty<RewardPackageId>();

        public static ProgressionSeed CreateResource()
        {
            return new ProgressionSeed(
                STARTING_FLAGS,
                STARTING_UNLOCKED_MODULES,
                STARTING_AVAILABLE_REWARDS);
        }
    }
}
