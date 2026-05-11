using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Frontier
{
    public sealed class Stage1FrontierSeed : IResource
    {
        public Stage1FrontierSeed(
            Stage1FrontierBotSpawnSeed[] initialBotSpawns,
            RegionId[] startingUnlockedRegions,
            ExpeditionId[] startingUnlockedExpeditions,
            RaidId[] startingUnlockedRaids)
        {
            InitialBotSpawns = CloneOrEmpty(initialBotSpawns);
            StartingUnlockedRegions = CloneOrEmpty(startingUnlockedRegions);
            StartingUnlockedExpeditions = CloneOrEmpty(startingUnlockedExpeditions);
            StartingUnlockedRaids = CloneOrEmpty(startingUnlockedRaids);
        }

        public Stage1FrontierBotSpawnSeed[] InitialBotSpawns { get; }
        public RegionId[] StartingUnlockedRegions { get; }
        public ExpeditionId[] StartingUnlockedExpeditions { get; }
        public RaidId[] StartingUnlockedRaids { get; }

        private static T[] CloneOrEmpty<T>(T[] source)
        {
            return source == null
                ? Array.Empty<T>()
                : (T[])source.Clone();
        }
    }
}
