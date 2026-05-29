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

            var count = 0;
            foreach (var recipe in AvailableRecipesQuery.Filter(ProductionRecipeCatalog.All))
            {
                if (recipe.StationId != ProductionStationIds.Workbench)
                    continue;

                if (count > 0)
                    builder.Append(", ");

                builder.Append(recipe.Code);
                count++;
            }

            if (count == 0)
                builder.Append("No unlocked recipes");

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
