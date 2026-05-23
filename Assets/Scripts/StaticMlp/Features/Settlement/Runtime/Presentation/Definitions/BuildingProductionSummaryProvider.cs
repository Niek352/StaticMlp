using System.Text;
using StaticMlp.Features.BuildingCatalog;

namespace StaticMlp.Features.Settlement
{
    internal sealed class BuildingProductionSummaryProvider : IBuildingSummaryProvider
    {
        public bool CanBuildSummary(BuildingInteractionKind kind, in BuildingDefinition definition)
        {
            return IsProductionInteraction(kind)
                   && definition.Capabilities.HasFlag(BuildingCapabilityFlags.OpensQueue);
        }

        public string BuildSummary(BuildingInteractionKind kind, in BuildingDefinition definition)
        {
            var builder = new StringBuilder();
            builder.Append("Worker slots: ");
            builder.Append(definition.Operation.WorkerSlots);
            builder.Append("\nRecipes: ");

            for (var i = 0; i < WorkbenchRecipeCatalog.All.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");

                builder.Append(WorkbenchRecipeCatalog.All[i].Code);
            }

            return builder.ToString();
        }

        private static bool IsProductionInteraction(BuildingInteractionKind kind)
        {
            return kind == BuildingInteractionKind.OpenProductionQueue
                   || kind == BuildingInteractionKind.SetRecipe
                   || kind == BuildingInteractionKind.ClaimOutput;
        }
    }
}
