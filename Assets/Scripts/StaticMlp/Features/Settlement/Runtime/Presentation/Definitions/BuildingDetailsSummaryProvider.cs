using System.Text;
using StaticMlp.Features.BuildingCatalog;

namespace StaticMlp.Features.Settlement
{
    internal sealed class BuildingDetailsSummaryProvider : IBuildingSummaryProvider
    {
        public bool CanBuildSummary(BuildingInteractionKind kind, in BuildingDefinition definition)
        {
            return kind == BuildingInteractionKind.OpenDetails;
        }

        public string BuildSummary(BuildingInteractionKind kind, in BuildingDefinition definition)
        {
            return
                $"Category: {definition.CategoryDisplayName}\n" +
                $"Footprint: {definition.FootprintWidth}x{definition.FootprintLength}\n" +
                $"Cost: {FormatResourceAmounts(definition.ConstructionCost)}";
        }

        private static string FormatResourceAmounts(ResourceAmount[] amounts)
        {
            var builder = new StringBuilder();

            for (var i = 0; i < amounts.Length; i++)
            {
                if (i > 0)
                    builder.Append(", ");

                builder.Append(ResourceCatalog.Get(amounts[i].Id).DisplayName);
                builder.Append(' ');
                builder.Append(amounts[i].Amount);
            }

            return builder.ToString();
        }

    }
}
