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

        public static string ResolveInputHint(BuildingInteractionKind kind, bool isPrimaryAction)
        {
            if (!isPrimaryAction)
                return "Building panel secondary button";

            switch (kind)
            {
                case BuildingInteractionKind.DepositConstructionResources:
                    return "Building panel primary button";
                case BuildingInteractionKind.ContributeBuildWork:
                    return "Building panel primary button / BuildConstruction hold";
                default:
                    return "Building panel primary button";
            }
        }

        public static string ResolveEffectDescription(BuildingInteractionKind kind)
        {
            switch (kind)
            {
                case BuildingInteractionKind.OpenDetails:
                    return "Opens the building details summary.";
                case BuildingInteractionKind.DepositConstructionResources:
                    return "Sends a request to deposit the remaining required construction resources.";
                case BuildingInteractionKind.ContributeBuildWork:
                    return "Sends a request to add construction work to the focused site.";
                case BuildingInteractionKind.AssignWorker:
                    return "Opens the worker assignment operation summary.";
                case BuildingInteractionKind.OpenProductionQueue:
                    return "Opens the production queue operation summary.";
                case BuildingInteractionKind.SetRecipe:
                    return "Opens the recipe selection operation summary.";
                case BuildingInteractionKind.ClaimOutput:
                    return "Opens the production output operation summary.";
                case BuildingInteractionKind.AssignBed:
                    return "Opens the bed assignment operation summary.";
                case BuildingInteractionKind.ToggleEnabled:
                    return "Opens the enabled-state operation summary.";
                case BuildingInteractionKind.TriggerRepair:
                    return "Opens the repair operation summary.";
                case BuildingInteractionKind.Extract:
                    return "Opens the extraction buffer operation summary.";
                case BuildingInteractionKind.Rest:
                    return "Opens the rest operation summary.";
                case BuildingInteractionKind.StoreItems:
                    return "Opens the storage operation summary.";
                case BuildingInteractionKind.WithdrawItems:
                    return "Opens the withdraw operation summary.";
                default:
                    return string.Empty;
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
