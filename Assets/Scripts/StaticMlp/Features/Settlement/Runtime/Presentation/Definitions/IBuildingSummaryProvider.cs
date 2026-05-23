using StaticMlp.Features.BuildingCatalog;

namespace StaticMlp.Features.Settlement
{
    public interface IBuildingSummaryProvider
    {
        bool CanBuildSummary(BuildingInteractionKind kind, in BuildingDefinition definition);
        string BuildSummary(BuildingInteractionKind kind, in BuildingDefinition definition);
    }
}
