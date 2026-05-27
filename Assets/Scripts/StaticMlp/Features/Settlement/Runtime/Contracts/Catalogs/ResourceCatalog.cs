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
        public static readonly ResourceId OreId = new(10);
        public static readonly ResourceId ResinId = new(11);
        public static readonly ResourceId SporesId = new(12);
        public static readonly ResourceId IngotsId = new(13);

        private static readonly ResourceDefinition[] Definitions =
        {
            new(
                WoodId,
                "Wood",
                ResourceFamily.Raw,
                ResourceUsageFlags.Construction | ResourceUsageFlags.ExpeditionReward | ResourceUsageFlags.ProductionInput | ResourceUsageFlags.ProductionOutput,
                isSettlementStored: true,
                startingSettlementAmount: 50),
            new(
                StoneId,
                "Stone",
                ResourceFamily.Raw,
                ResourceUsageFlags.Construction | ResourceUsageFlags.ExpeditionReward | ResourceUsageFlags.Repair | ResourceUsageFlags.ProductionInput | ResourceUsageFlags.ProductionOutput,
                isSettlementStored: true,
                startingSettlementAmount: 25),
            new(
                PlanksId,
                "Planks",
                ResourceFamily.Refined,
                ResourceUsageFlags.Construction | ResourceUsageFlags.ProductionOutput | ResourceUsageFlags.ProductionInput,
                isSettlementStored: true,
                startingSettlementAmount: 0),
            new(
                SimplePartsId,
                "Simple Parts",
                ResourceFamily.Refined,
                ResourceUsageFlags.Construction | ResourceUsageFlags.Repair | ResourceUsageFlags.ProductionOutput | ResourceUsageFlags.ProductionInput,
                isSettlementStored: true,
                startingSettlementAmount: 0),
            new(
                RepairKitsId,
                "Repair Kits",
                ResourceFamily.Stability,
                ResourceUsageFlags.Repair | ResourceUsageFlags.ProductionOutput,
                isSettlementStored: true,
                startingSettlementAmount: 0),
            new(
                FoodId,
                "Food",
                ResourceFamily.Flow,
                ResourceUsageFlags.Upkeep | ResourceUsageFlags.ExpeditionReward,
                isSettlementStored: true,
                startingSettlementAmount: 0),
            new(
                FuelId,
                "Fuel",
                ResourceFamily.Flow,
                ResourceUsageFlags.Fuel | ResourceUsageFlags.ExpeditionReward | ResourceUsageFlags.ProductionInput,
                isSettlementStored: true,
                startingSettlementAmount: 0),
            new(
                ResearchDataId,
                "Research Data",
                ResourceFamily.Progression,
                ResourceUsageFlags.Progression | ResourceUsageFlags.ExpeditionReward,
                isSettlementStored: true,
                startingSettlementAmount: 0),
            new(
                MedicineId,
                "Medicine",
                ResourceFamily.Stability,
                ResourceUsageFlags.Upkeep | ResourceUsageFlags.ProductionOutput,
                isSettlementStored: true,
                startingSettlementAmount: 0),
            new(
                OreId,
                "Ore",
                ResourceFamily.Raw,
                ResourceUsageFlags.ExpeditionReward | ResourceUsageFlags.ProductionInput | ResourceUsageFlags.ProductionOutput,
                isSettlementStored: true,
                startingSettlementAmount: 0),
            new(
                ResinId,
                "Resin",
                ResourceFamily.Raw,
                ResourceUsageFlags.ExpeditionReward | ResourceUsageFlags.ProductionInput | ResourceUsageFlags.ProductionOutput,
                isSettlementStored: true,
                startingSettlementAmount: 0),
            new(
                SporesId,
                "Spores",
                ResourceFamily.Raw,
                ResourceUsageFlags.ExpeditionReward | ResourceUsageFlags.ProductionInput | ResourceUsageFlags.ProductionOutput,
                isSettlementStored: true,
                startingSettlementAmount: 0),
            new(
                IngotsId,
                "Ingots",
                ResourceFamily.Refined,
                ResourceUsageFlags.Construction | ResourceUsageFlags.Repair | ResourceUsageFlags.ProductionInput | ResourceUsageFlags.ProductionOutput,
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
