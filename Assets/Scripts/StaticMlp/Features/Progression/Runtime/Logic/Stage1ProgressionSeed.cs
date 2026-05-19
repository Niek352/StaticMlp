using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Loadout;

namespace StaticMlp.Features.Progression
{
    public sealed class Stage1ProgressionSeed : IResource
    {
        public Stage1ProgressionSeed(
            ProgressFlagId[] startingFlags,
            LoadoutModuleId[] startingUnlockedModules,
            RewardPackageId[] startingAvailableRewards)
        {
            StartingFlags = CloneOrEmpty(startingFlags);
            StartingUnlockedModules = CloneOrEmpty(startingUnlockedModules);
            StartingAvailableRewards = CloneOrEmpty(startingAvailableRewards);
        }

        public ProgressFlagId[] StartingFlags { get; }
        public LoadoutModuleId[] StartingUnlockedModules { get; }
        public RewardPackageId[] StartingAvailableRewards { get; }

        private static T[] CloneOrEmpty<T>(T[] source)
        {
            return source == null
                ? Array.Empty<T>()
                : (T[])source.Clone();
        }
    }
}
