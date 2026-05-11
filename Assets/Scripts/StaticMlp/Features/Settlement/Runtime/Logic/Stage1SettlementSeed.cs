using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public sealed class Stage1SettlementSeed : IResource
    {
        public Stage1SettlementSeed(
            Stage1ConstructionSiteSeed[] initialConstructionSites,
            ResourceAmount[] startingResources,
            Stage1SettlementWorkerSeed[] initialWorkers)
        {
            InitialConstructionSites = CloneOrEmpty(initialConstructionSites);
            StartingResources = CloneOrEmpty(startingResources);
            InitialWorkers = CloneOrEmpty(initialWorkers);
        }

        public Stage1ConstructionSiteSeed[] InitialConstructionSites { get; }
        public ResourceAmount[] StartingResources { get; }
        public Stage1SettlementWorkerSeed[] InitialWorkers { get; }

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
