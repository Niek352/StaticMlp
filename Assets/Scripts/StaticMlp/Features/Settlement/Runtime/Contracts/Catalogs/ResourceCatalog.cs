using System;
using System.Collections.Generic;

namespace StaticMlp.Features.Settlement
{
    public static class ResourceCatalog
    {
        public static readonly ResourceId WoodId = new(1);
        public static readonly ResourceId StoneId = new(2);
        public static readonly ResourceId PlanksId = new(3);
        public static readonly ResourceId SimplePartsId = new(4);
        public static readonly ResourceId RepairKitsId = new(5);
        public static readonly ResourceId FoodId = new(6);
        public static readonly ResourceId FuelId = new(7);
        public static readonly ResourceId ResearchDataId = new(8);
        public static readonly ResourceId MedicineId = new(9);

        private static readonly ResourceDefinition[] Definitions =
        {
            new(
                WoodId,
                ResourceFamily.Raw,
                ResourceUsageFlags.Construction | ResourceUsageFlags.ExpeditionReward | ResourceUsageFlags.ProductionInput,
                isSettlementStored: true,
                startingSettlementAmount: 50),
            new(
                StoneId,
                ResourceFamily.Raw,
                ResourceUsageFlags.Construction | ResourceUsageFlags.ExpeditionReward | ResourceUsageFlags.Repair | ResourceUsageFlags.ProductionInput,
                isSettlementStored: true,
                startingSettlementAmount: 25),
            new(
                PlanksId,
                ResourceFamily.Refined,
                ResourceUsageFlags.Construction | ResourceUsageFlags.ProductionOutput | ResourceUsageFlags.ProductionInput,
                isSettlementStored: true,
                startingSettlementAmount: 0),
            new(
                SimplePartsId,
                ResourceFamily.Refined,
                ResourceUsageFlags.Construction | ResourceUsageFlags.Repair | ResourceUsageFlags.ProductionOutput | ResourceUsageFlags.ProductionInput,
                isSettlementStored: true,
                startingSettlementAmount: 0),
            new(
                RepairKitsId,
                ResourceFamily.Stability,
                ResourceUsageFlags.Repair | ResourceUsageFlags.ProductionOutput,
                isSettlementStored: true,
                startingSettlementAmount: 0),
            new(
                FoodId,
                ResourceFamily.Flow,
                ResourceUsageFlags.Upkeep | ResourceUsageFlags.ExpeditionReward,
                isSettlementStored: true,
                startingSettlementAmount: 0),
            new(
                FuelId,
                ResourceFamily.Flow,
                ResourceUsageFlags.Fuel | ResourceUsageFlags.ExpeditionReward | ResourceUsageFlags.ProductionInput,
                isSettlementStored: true,
                startingSettlementAmount: 0),
            new(
                ResearchDataId,
                ResourceFamily.Progression,
                ResourceUsageFlags.Progression | ResourceUsageFlags.ExpeditionReward,
                isSettlementStored: true,
                startingSettlementAmount: 0),
            new(
                MedicineId,
                ResourceFamily.Stability,
                ResourceUsageFlags.Upkeep | ResourceUsageFlags.ProductionOutput,
                isSettlementStored: true,
                startingSettlementAmount: 0)
        };

        static ResourceCatalog()
        {
            ResourceCatalogValidator.Validate(Definitions);
        }

        public static IReadOnlyList<ResourceDefinition> All => Definitions;

        public static ref readonly ResourceDefinition Get(ResourceId id)
        {
            for (var i = 0; i < Definitions.Length; i++)
            {
                if (Definitions[i].Id != id)
                    continue;

                return ref Definitions[i];
            }

            throw new InvalidOperationException($"Missing {nameof(ResourceDefinition)} for resource id {id.Value} in {nameof(ResourceCatalog)}.");
        }
    }
}
