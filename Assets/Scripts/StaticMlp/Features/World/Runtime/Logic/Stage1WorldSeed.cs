using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.World
{
    public sealed class Stage1WorldSeed : IResource
    {
        public Stage1WorldSeed(
            Stage1BotSpawnSeed[] initialBotSpawns,
            RegionId[] startingUnlockedRegions,
            ExpeditionId[] startingUnlockedExpeditions,
            RaidId[] startingUnlockedRaids)
        {
            InitialBotSpawns = CloneOrEmpty(initialBotSpawns);
            StartingUnlockedRegions = CloneOrEmpty(startingUnlockedRegions);
            StartingUnlockedExpeditions = CloneOrEmpty(startingUnlockedExpeditions);
            StartingUnlockedRaids = CloneOrEmpty(startingUnlockedRaids);
        }

        public Stage1BotSpawnSeed[] InitialBotSpawns { get; }
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
