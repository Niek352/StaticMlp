using StaticMlp.Features.BuildingCatalog;

namespace StaticMlp.Features.Settlement
{
    public static class BuildingActionPresentationCatalog
    {
        private static readonly BuildingInteractionPresentationDefinition[] Definitions =
        {
            new(
                BuildingInteractionKind.OpenDetails,
                "Building panel primary button",
                "Opens the building details summary."),
            new(
                BuildingInteractionKind.DepositConstructionResources,
                "Building panel primary button",
                "Sends a request to deposit the remaining required construction resources."),
            new(
                BuildingInteractionKind.ContributeBuildWork,
                "Building panel primary button / BuildConstruction hold",
                "Sends a request to add construction work to the focused site."),
            new(
                BuildingInteractionKind.AssignWorker,
                "Building panel primary button",
                "Opens the worker assignment operation summary."),
            new(
                BuildingInteractionKind.OpenProductionQueue,
                "Building panel primary button",
                "Opens the production queue operation summary."),
            new(
                BuildingInteractionKind.SetRecipe,
                "Building panel primary button",
                "Opens the recipe selection operation summary."),
            new(
                BuildingInteractionKind.ClaimOutput,
                "Building panel primary button",
                "Opens the production output operation summary."),
            new(
                BuildingInteractionKind.AssignBed,
                "Building panel primary button",
                "Opens the bed assignment operation summary."),
            new(
                BuildingInteractionKind.ToggleEnabled,
                "Building panel primary button",
                "Opens the enabled-state operation summary."),
            new(
                BuildingInteractionKind.TriggerRepair,
                "Building panel primary button",
                "Opens the repair operation summary."),
            new(
                BuildingInteractionKind.Extract,
                "Building panel primary button",
                "Opens the extraction buffer operation summary."),
            new(
                BuildingInteractionKind.Rest,
                "Building panel primary button",
                "Opens the rest operation summary."),
            new(
                BuildingInteractionKind.StoreItems,
                "Building panel primary button",
                "Sends a request to deposit carried raw resources into shared storage."),
            new(
                BuildingInteractionKind.WithdrawItems,
                "Building panel primary button",
                "Opens the withdraw operation summary.")
        };

        public static string ResolveSummary(BuildingInteractionKind kind, in BuildingDefinition definition)
        {
            return BuildingSummaryProviderRegistry.Resolve(kind, in definition);
        }

        public static string ResolveInputHint(BuildingInteractionKind kind, bool isPrimaryAction)
        {
            if (!isPrimaryAction)
                return "Building panel secondary button";

            return Get(kind).PrimaryInputHint;
        }

        public static string ResolveEffectDescription(BuildingInteractionKind kind)
        {
            return Get(kind).EffectDescription;
        }

        private static BuildingInteractionPresentationDefinition Get(BuildingInteractionKind kind)
        {
            for (var i = 0; i < Definitions.Length; i++)
            {
                if (Definitions[i].Kind == kind)
                    return Definitions[i];
            }

            throw new System.InvalidOperationException($"Missing presentation definition for building interaction {kind}.");
        }
    }
}
