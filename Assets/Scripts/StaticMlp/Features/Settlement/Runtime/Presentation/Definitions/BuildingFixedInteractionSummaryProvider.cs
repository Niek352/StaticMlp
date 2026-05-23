using StaticMlp.Features.BuildingCatalog;

namespace StaticMlp.Features.Settlement
{
    internal sealed class BuildingFixedInteractionSummaryProvider : IBuildingSummaryProvider
    {
        public bool CanBuildSummary(BuildingInteractionKind kind, in BuildingDefinition definition)
        {
            return kind == BuildingInteractionKind.ToggleEnabled
                   || kind == BuildingInteractionKind.TriggerRepair;
        }

        public string BuildSummary(BuildingInteractionKind kind, in BuildingDefinition definition)
        {
            if (kind == BuildingInteractionKind.ToggleEnabled)
                return "Toggle operation enabled state.";

            if (kind == BuildingInteractionKind.TriggerRepair)
                return "Repair operation available.";

            throw new System.InvalidOperationException($"Unsupported fixed building interaction summary {kind}.");
        }
    }
}
