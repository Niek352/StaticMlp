using StaticMlp.Features.BuildingCatalog;

namespace StaticMlp.Features.Settlement
{
    internal sealed class BuildingExtractionSummaryProvider : IBuildingSummaryProvider
    {
        public bool CanBuildSummary(BuildingInteractionKind kind, in BuildingDefinition definition)
        {
            return kind == BuildingInteractionKind.Extract
                   && definition.Capabilities.HasFlag(BuildingCapabilityFlags.ExtractsFromNode);
        }

        public string BuildSummary(BuildingInteractionKind kind, in BuildingDefinition definition)
        {
            if (!ExtractionRules.TryGetOutputResource(definition.Id, out var output))
                throw new System.InvalidOperationException(
                    $"Extraction action requested for non-extraction building {definition.Id.Value}.");

            return
                $"Output: {ResourceCatalog.Get(output).DisplayName}\n" +
                $"Buffer capacity: {definition.Operation.StorageCapacity}\n" +
                $"Worker slots: {definition.Operation.WorkerSlots}";
        }
    }
}
