using StaticMlp.Features.BuildingCatalog;

namespace StaticMlp.Features.Settlement
{
    internal sealed class BuildingWorkerSummaryProvider : IBuildingSummaryProvider
    {
        public bool CanBuildSummary(BuildingInteractionKind kind, in BuildingDefinition definition)
        {
            return kind == BuildingInteractionKind.AssignWorker
                   && definition.Capabilities.HasFlag(BuildingCapabilityFlags.SupportsNpcInteraction);
        }

        public string BuildSummary(BuildingInteractionKind kind, in BuildingDefinition definition)
        {
            return $"Worker slots: {definition.NpcProfile.WorkerSlots}";
        }
    }
}
