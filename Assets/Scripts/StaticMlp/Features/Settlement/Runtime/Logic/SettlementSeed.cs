using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementSeed : IResource
    {
        public SettlementSeed(
            SettlementConstructionSiteSeed[] initialConstructionSites,
            ResourceAmount[] startingResources,
            SettlementWorkerSeed[] initialWorkers)
        {
            InitialConstructionSites = CloneOrEmpty(initialConstructionSites);
            StartingResources = CloneOrEmpty(startingResources);
            InitialWorkers = CloneOrEmpty(initialWorkers);
        }

        public SettlementConstructionSiteSeed[] InitialConstructionSites { get; }
        public ResourceAmount[] StartingResources { get; }
        public SettlementWorkerSeed[] InitialWorkers { get; }

        public int GetStartingResourceAmount(ResourceId resourceId)
        {
            for (var i = 0; i < StartingResources.Length; i++)
            {
                if (StartingResources[i].Id != resourceId)
                    continue;

                return StartingResources[i].Amount;
            }

            throw new InvalidOperationException($"Missing starting amount for settlement resource id {resourceId.Value}.");
        }

        private static T[] CloneOrEmpty<T>(T[] source)
        {
            return source == null
                ? Array.Empty<T>()
                : (T[])source.Clone();
        }
    }
}
