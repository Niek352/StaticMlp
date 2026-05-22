using System.Text;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;

namespace StaticMlp.Features.Settlement
{
    public static class BuildingActionPresentationCatalog
    {
        public static string ResolveLabel(BuildingInteractionKind kind)
        {
            switch (kind)
            {
                case BuildingInteractionKind.OpenDetails:
                    return "Open";
                case BuildingInteractionKind.DepositConstructionResources:
                    return "Deposit";
                case BuildingInteractionKind.ContributeBuildWork:
                    return "Build";
                case BuildingInteractionKind.AssignWorker:
                    return "Assign Worker";
                case BuildingInteractionKind.OpenProductionQueue:
                    return "Open Queue";
                case BuildingInteractionKind.SetRecipe:
                    return "Set Recipe";
                case BuildingInteractionKind.ClaimOutput:
                    return "Claim Output";
                case BuildingInteractionKind.AssignBed:
                    return "Assign Bed";
                case BuildingInteractionKind.ToggleEnabled:
                    return "Toggle";
                case BuildingInteractionKind.TriggerRepair:
                    return "Repair";
                case BuildingInteractionKind.Extract:
                    return "Extract";
                case BuildingInteractionKind.Rest:
                    return "Rest";
                case BuildingInteractionKind.StoreItems:
                    return "Open Storage";
                case BuildingInteractionKind.WithdrawItems:
                    return "Withdraw";
                default:
                    return string.Empty;
            }
        }

        public static string ResolveSummary(BuildingInteractionKind kind, in BuildingDefinition definition)
        {
            switch (kind)
            {
                case BuildingInteractionKind.OpenDetails:
                    return BuildDetailsSummary(in definition);
                case BuildingInteractionKind.AssignWorker:
                    return $"Worker slots: {definition.NpcProfile.WorkerSlots}";
                case BuildingInteractionKind.OpenProductionQueue:
                case BuildingInteractionKind.SetRecipe:
                case BuildingInteractionKind.ClaimOutput:
                    return BuildProductionSummary(in definition);
                case BuildingInteractionKind.AssignBed:
                case BuildingInteractionKind.Rest:
                    return $"Bed slots: {definition.Operation.WorkerSlots}";
                case BuildingInteractionKind.Extract:
                    return BuildExtractionSummary(in definition);
                case BuildingInteractionKind.StoreItems:
                case BuildingInteractionKind.WithdrawItems:
                    return $"Storage capacity: {definition.Operation.StorageCapacity}";
                case BuildingInteractionKind.ToggleEnabled:
                    return "Toggle operation enabled state.";
                case BuildingInteractionKind.TriggerRepair:
                    return "Repair operation available.";
                default:
                    return BuildDetailsSummary(in definition);
            }
        }

        private static string BuildDetailsSummary(in BuildingDefinition definition)
        {
            return
                $"Category: {ResolveCategoryLabel(definition.Category)}\n" +
                $"Footprint: {definition.FootprintWidth}x{definition.FootprintLength}\n" +
                $"Cost: {FormatResourceAmounts(definition.ConstructionCost)}";
        }

        private static string BuildProductionSummary(in BuildingDefinition definition)
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

        private static string BuildExtractionSummary(in BuildingDefinition definition)
        {
            if (!ExtractionRules.TryGetOutputResource(definition.Id, out var output))
                throw new System.InvalidOperationException($"Extraction action requested for non-extraction building {definition.Id.Value}.");

            return
                $"Output: {ResourceCatalog.Get(output).DisplayName}\n" +
                $"Buffer capacity: {definition.Operation.StorageCapacity}\n" +
                $"Worker slots: {definition.Operation.WorkerSlots}";
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

        private static string ResolveCategoryLabel(BuildingCategory category)
        {
            switch (category)
            {
                case BuildingCategory.Housing:
                    return "Housing";
                case BuildingCategory.Logistics:
                    return "Logistics";
                case BuildingCategory.Extraction:
                    return "Extraction";
                case BuildingCategory.Production:
                    return "Production";
                case BuildingCategory.Service:
                    return "Service";
                case BuildingCategory.Defense:
                    return "Defense";
                case BuildingCategory.Research:
                    return "Research";
                default:
                    return "Uncategorized";
            }
        }
    }
}
