using StaticMlp.Features.BuildingCatalog;

namespace StaticMlp.Features.Settlement
{
    public static class BuildingSummaryProviderRegistry
    {
        private static readonly IBuildingSummaryProvider[] Providers =
        {
            new BuildingDetailsSummaryProvider(),
            new BuildingWorkerSummaryProvider(),
            new BuildingProductionSummaryProvider(),
            new BuildingHousingSummaryProvider(),
            new BuildingExtractionSummaryProvider(),
            new BuildingStorageSummaryProvider(),
            new BuildingFixedInteractionSummaryProvider()
        };

        public static string Resolve(BuildingInteractionKind kind, in BuildingDefinition definition)
        {
            for (var i = 0; i < Providers.Length; i++)
            {
                if (Providers[i].CanBuildSummary(kind, in definition))
                    return Providers[i].BuildSummary(kind, in definition);
            }

            throw new System.InvalidOperationException(
                $"Building {definition.Id.Value} has no summary provider for interaction {kind}.");
        }
    }
}
