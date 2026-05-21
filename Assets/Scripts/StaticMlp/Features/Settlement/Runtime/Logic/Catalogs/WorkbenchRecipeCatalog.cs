using System;
using System.Collections.Generic;

namespace StaticMlp.Features.Settlement
{
    public static class WorkbenchRecipeCatalog
    {
        public static readonly WorkbenchRecipeId PlanksId = new(1);
        public static readonly WorkbenchRecipeId SimplePartsId = new(2);
        public static readonly WorkbenchRecipeId RepairKitsId = new(3);

        private static readonly WorkbenchRecipeDefinition[] Definitions =
        {
            new(
                PlanksId,
                "planks",
                new[] { new ResourceAmount(ResourceCatalog.WoodId, 2) },
                new[] { new ResourceAmount(ResourceCatalog.PlanksId, 1) },
                workRequired: 20f),
            new(
                SimplePartsId,
                "simple_parts",
                new[]
                {
                    new ResourceAmount(ResourceCatalog.WoodId, 1),
                    new ResourceAmount(ResourceCatalog.StoneId, 1)
                },
                new[] { new ResourceAmount(ResourceCatalog.SimplePartsId, 1) },
                workRequired: 30f),
            new(
                RepairKitsId,
                "repair_kits",
                new[]
                {
                    new ResourceAmount(ResourceCatalog.PlanksId, 2),
                    new ResourceAmount(ResourceCatalog.SimplePartsId, 1)
                },
                new[] { new ResourceAmount(ResourceCatalog.RepairKitsId, 1) },
                workRequired: 45f)
        };

        static WorkbenchRecipeCatalog()
        {
            Validate(Definitions);
        }

        public static IReadOnlyList<WorkbenchRecipeDefinition> All => Definitions;

        public static WorkbenchRecipeDefinition Get(WorkbenchRecipeId id)
        {
            for (var i = 0; i < Definitions.Length; i++)
            {
                if (Definitions[i].Id == id)
                    return Definitions[i];
            }

            throw new InvalidOperationException($"Missing workbench recipe id {id.Value}.");
        }

        public static void Validate(IReadOnlyList<WorkbenchRecipeDefinition> definitions)
        {
            var ids = new HashSet<WorkbenchRecipeId>();
            var codes = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                if (definition.Id.Value == 0)
                    throw new InvalidOperationException("Workbench recipe id 0 is reserved.");

                if (!ids.Add(definition.Id))
                    throw new InvalidOperationException($"Duplicate workbench recipe id {definition.Id.Value}.");

                if (string.IsNullOrWhiteSpace(definition.Code))
                    throw new InvalidOperationException($"Workbench recipe id {definition.Id.Value} is missing a code.");

                if (!codes.Add(definition.Code))
                    throw new InvalidOperationException($"Duplicate workbench recipe code {definition.Code}.");

                ValidateAmounts(definition.Id, definition.Inputs, "input");
                ValidateAmounts(definition.Id, definition.Outputs, "output");
            }
        }

        private static void ValidateAmounts(WorkbenchRecipeId recipeId, ResourceAmount[] amounts, string role)
        {
            if (amounts == null || amounts.Length == 0)
                throw new InvalidOperationException($"Workbench recipe id {recipeId.Value} must define at least one {role}.");

            var ids = new HashSet<ResourceId>();
            for (var i = 0; i < amounts.Length; i++)
            {
                var amount = amounts[i];
                ResourceCatalog.Get(amount.Id);
                if (amount.Amount <= 0)
                    throw new InvalidOperationException($"Workbench recipe id {recipeId.Value} has non-positive {role} amount {amount.Amount}.");

                if (!ids.Add(amount.Id))
                    throw new InvalidOperationException($"Workbench recipe id {recipeId.Value} has duplicate {role} resource id {amount.Id.Value}.");
            }
        }
    }
}
