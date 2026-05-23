using StaticMlp.Features.BuildingCatalog;

namespace StaticMlp.Features.Settlement
{
    internal sealed class BuildingHousingSummaryProvider : IBuildingSummaryProvider
    {
        public bool CanBuildSummary(BuildingInteractionKind kind, in BuildingDefinition definition)
        {
            return IsHousingInteraction(kind)
                   && (definition.Capabilities.HasFlag(BuildingCapabilityFlags.ProvidesHousing)
                       || definition.Capabilities.HasFlag(BuildingCapabilityFlags.ProvidesRest));
        }

        public string BuildSummary(BuildingInteractionKind kind, in BuildingDefinition definition)
        {
            return $"Bed slots: {definition.Operation.WorkerSlots}";
        }

        private static bool IsHousingInteraction(BuildingInteractionKind kind)
        {
            return kind == BuildingInteractionKind.AssignBed
                   || kind == BuildingInteractionKind.Rest;
        }
    }
}
