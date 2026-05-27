using System;
using System.Collections.Generic;

namespace StaticMlp.Features.Settlement
{
    public static class ProductionRecipeCatalog
    {
        public static readonly ProductionRecipeId WorkbenchPlanksId = new(1);
        public static readonly ProductionRecipeId WorkbenchSimplePartsId = new(2);
        public static readonly ProductionRecipeId WorkbenchRepairKitsId = new(3);

        private static readonly ProductionRecipeDefinition[] Definitions =
        {
            new(
                ProductionStationIds.Workbench,
                WorkbenchPlanksId,
                "planks",
                new[] { new ResourceAmount(ResourceCatalog.WoodId, 2) },
                new[] { new ResourceAmount(ResourceCatalog.PlanksId, 1) },
                workRequired: 20f),
            new(
                ProductionStationIds.Workbench,
                WorkbenchSimplePartsId,
                "simple_parts",
                new[]
                {
                    new ResourceAmount(ResourceCatalog.WoodId, 1),
                    new ResourceAmount(ResourceCatalog.StoneId, 1)
                },
                new[] { new ResourceAmount(ResourceCatalog.SimplePartsId, 1) },
                workRequired: 30f),
            new(
                ProductionStationIds.Workbench,
                WorkbenchRepairKitsId,
                "repair_kits",
                new[]
                {
                    new ResourceAmount(ResourceCatalog.PlanksId, 2),
                    new ResourceAmount(ResourceCatalog.SimplePartsId, 1)
                },
                new[] { new ResourceAmount(ResourceCatalog.RepairKitsId, 1) },
                workRequired: 45f)
        };

        static ProductionRecipeCatalog()
        {
            Validate(Definitions);
        }

        public static IReadOnlyList<ProductionRecipeDefinition> All => Definitions;

        public static ProductionRecipeDefinition Get(ProductionRecipeId id)
        {
            for (var i = 0; i < Definitions.Length; i++)
            {
                if (Definitions[i].Id == id)
                    return Definitions[i];
            }

            throw new InvalidOperationException($"Missing production recipe id {id.Value}.");
        }

        public static ProductionRecipeDefinition Get(ProductionStationId stationId, ProductionRecipeId id)
        {
            var definition = Get(id);
            if (definition.StationId != stationId)
                throw new InvalidOperationException(
                    $"Production recipe id {id.Value} does not belong to station id {stationId.Value}.");

            return definition;
        }

        public static void Validate(IReadOnlyList<ProductionRecipeDefinition> definitions)
        {
            var ids = new HashSet<ProductionRecipeId>();
            var codesByStation = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                if (definition.StationId.Value == 0)
                    throw new InvalidOperationException("Production station id 0 is reserved.");

                if (definition.Id.Value == 0)
                    throw new InvalidOperationException("Production recipe id 0 is reserved.");

                if (!ids.Add(definition.Id))
                    throw new InvalidOperationException($"Duplicate production recipe id {definition.Id.Value}.");

                if (string.IsNullOrWhiteSpace(definition.Code))
                    throw new InvalidOperationException($"Production recipe id {definition.Id.Value} is missing a code.");

                var stationCodeKey = $"{definition.StationId.Value}:{definition.Code}";
                if (!codesByStation.Add(stationCodeKey))
                    throw new InvalidOperationException(
                        $"Duplicate production recipe code {definition.Code} for station {definition.StationId.Value}.");

                if (definition.WorkRequired <= 0f)
                    throw new InvalidOperationException(
                        $"Production recipe id {definition.Id.Value} has non-positive work requirement {definition.WorkRequired}.");

                ValidateAmounts(definition.Id, definition.Inputs, "input");
                ValidateAmounts(definition.Id, definition.Outputs, "output");
                ValidateFuel(definition);
            }
        }

        private static void ValidateAmounts(ProductionRecipeId recipeId, ResourceAmount[] amounts, string role)
        {
            if (amounts == null || amounts.Length == 0)
                throw new InvalidOperationException($"Production recipe id {recipeId.Value} must define at least one {role}.");

            var ids = new HashSet<ResourceId>();
            for (var i = 0; i < amounts.Length; i++)
            {
                var amount = amounts[i];
                ResourceCatalog.Get(amount.Id);
                if (amount.Amount <= 0)
                    throw new InvalidOperationException(
                        $"Production recipe id {recipeId.Value} has non-positive {role} amount {amount.Amount}.");

                if (!ids.Add(amount.Id))
                    throw new InvalidOperationException(
                        $"Production recipe id {recipeId.Value} has duplicate {role} resource id {amount.Id.Value}.");
            }
        }

        private static void ValidateFuel(in ProductionRecipeDefinition definition)
        {
            if (!definition.FuelRequirement.HasValue)
                return;

            var fuel = definition.FuelRequirement.Value;
            ResourceCatalog.Get(fuel.Id);
            if (fuel.Amount <= 0)
                throw new InvalidOperationException(
                    $"Production recipe id {definition.Id.Value} has non-positive fuel amount {fuel.Amount}.");
        }
    }
}
