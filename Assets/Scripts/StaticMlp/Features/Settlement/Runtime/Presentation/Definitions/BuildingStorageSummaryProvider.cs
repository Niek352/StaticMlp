using StaticMlp.Features.BuildingCatalog;

namespace StaticMlp.Features.Settlement
{
    internal sealed class BuildingStorageSummaryProvider : IBuildingSummaryProvider
    {
        public bool CanBuildSummary(BuildingInteractionKind kind, in BuildingDefinition definition)
        {
            return IsStorageInteraction(kind)
                   && definition.Capabilities.HasFlag(BuildingCapabilityFlags.ProvidesStorage);
        }

        public string BuildSummary(BuildingInteractionKind kind, in BuildingDefinition definition)
        {
            return $"Storage capacity: {definition.Operation.StorageCapacity}";
        }

        private static bool IsStorageInteraction(BuildingInteractionKind kind)
        {
            return kind == BuildingInteractionKind.StoreItems
                   || kind == BuildingInteractionKind.WithdrawItems;
        }
    }
}
