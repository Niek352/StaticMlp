using System;
using System.Collections.Generic;
using StaticMlp.Features.Settlement;
using Unity.Mathematics;

namespace StaticMlp.Features.BuildingCatalog
{
    public static class BuildingCatalogData
    {
        public static readonly BuildingId CampCoreId = new(1);
        public static readonly BuildingId StockpileId = new(2);
        public static readonly BuildingId BedrollShelterId = new(3);
        public static readonly BuildingId LumberCampId = new(4);
        public static readonly BuildingId StoneMineId = new(5);
        public static readonly BuildingId WorkbenchId = new(6);

        public static readonly BuildingId WoodenHutId = CampCoreId;

        private static readonly BuildingDefinition[] Definitions =
        {
            new(
                CampCoreId,
                code: "camp_core",
                displayName: "Camp Core",
                BuildingCategory.Service,
                BuildingCapabilityFlags.SupportsNpcInteraction
                    | BuildingCapabilityFlags.SupportsPlayerInteraction
                    | BuildingCapabilityFlags.BlocksPathing,
                new[]
                {
                    new ResourceAmount(ResourceCatalog.WoodId, 10),
                    new ResourceAmount(ResourceCatalog.StoneId, 4)
                },
                new int2(4, 5),
                buildWorkRequired: 100f,
                ConstructionInteractions(),
                new BuildingNpcProfileDefinition(supportsWorkers: true, workerSlots: 1),
                BuildingOperationDefinition.None),
            new(
                StockpileId,
                code: "stockpile",
                displayName: "Stockpile",
                BuildingCategory.Logistics,
                BuildingCapabilityFlags.ProvidesStorage
                    | BuildingCapabilityFlags.SupportsNpcInteraction
                    | BuildingCapabilityFlags.SupportsPlayerInteraction
                    | BuildingCapabilityFlags.BlocksPathing,
                new[]
                {
                    new ResourceAmount(ResourceCatalog.WoodId, 12),
                    new ResourceAmount(ResourceCatalog.StoneId, 2)
                },
                new int2(4, 4),
                buildWorkRequired: 80f,
                new[]
                {
                    new BuildingInteractionDefinition(BuildingInteractionKind.OpenDetails, requiresCompletedBuilding: false),
                    new BuildingInteractionDefinition(BuildingInteractionKind.DepositConstructionResources, requiresCompletedBuilding: false),
                    new BuildingInteractionDefinition(BuildingInteractionKind.ContributeBuildWork, requiresCompletedBuilding: false),
                    new BuildingInteractionDefinition(BuildingInteractionKind.StoreItems, requiresCompletedBuilding: true),
                    new BuildingInteractionDefinition(BuildingInteractionKind.WithdrawItems, requiresCompletedBuilding: true)
                },
                new BuildingNpcProfileDefinition(supportsWorkers: true, workerSlots: 1),
                new BuildingOperationDefinition(
                    BuildingCapabilityFlags.ProvidesStorage,
                    storageCapacity: 200,
                    workerSlots: 1)),
            new(
                BedrollShelterId,
                code: "bedroll_shelter",
                displayName: "Bedroll Shelter",
                BuildingCategory.Housing,
                BuildingCapabilityFlags.ProvidesHousing
                    | BuildingCapabilityFlags.ProvidesRest
                    | BuildingCapabilityFlags.SupportsNpcInteraction
                    | BuildingCapabilityFlags.SupportsPlayerInteraction
                    | BuildingCapabilityFlags.BlocksPathing,
                new[]
                {
                    new ResourceAmount(ResourceCatalog.WoodId, 10),
                    new ResourceAmount(ResourceCatalog.StoneId, 2)
                },
                new int2(5, 4),
                buildWorkRequired: 70f,
                new[]
                {
                    new BuildingInteractionDefinition(BuildingInteractionKind.OpenDetails, requiresCompletedBuilding: false),
                    new BuildingInteractionDefinition(BuildingInteractionKind.DepositConstructionResources, requiresCompletedBuilding: false),
                    new BuildingInteractionDefinition(BuildingInteractionKind.ContributeBuildWork, requiresCompletedBuilding: false),
                    new BuildingInteractionDefinition(BuildingInteractionKind.AssignBed, requiresCompletedBuilding: true),
                    new BuildingInteractionDefinition(BuildingInteractionKind.Rest, requiresCompletedBuilding: true)
                },
                new BuildingNpcProfileDefinition(supportsWorkers: true, workerSlots: 2),
                new BuildingOperationDefinition(
                    BuildingCapabilityFlags.ProvidesRest,
                    storageCapacity: 0,
                    workerSlots: 2)),
            new(
                LumberCampId,
                code: "lumber_camp",
                displayName: "Lumber Camp",
                BuildingCategory.Extraction,
                BuildingCapabilityFlags.ProvidesWorkplace
                    | BuildingCapabilityFlags.ProducesResources
                    | BuildingCapabilityFlags.ExtractsFromNode
                    | BuildingCapabilityFlags.SupportsNpcInteraction
                    | BuildingCapabilityFlags.SupportsPlayerInteraction
                    | BuildingCapabilityFlags.BlocksPathing,
                new[]
                {
                    new ResourceAmount(ResourceCatalog.WoodId, 12),
                    new ResourceAmount(ResourceCatalog.StoneId, 4)
                },
                new int2(5, 5),
                buildWorkRequired: 90f,
                ExtractionInteractions(),
                new BuildingNpcProfileDefinition(supportsWorkers: true, workerSlots: 2),
                new BuildingOperationDefinition(
                    BuildingCapabilityFlags.ProvidesWorkplace
                    | BuildingCapabilityFlags.ProducesResources
                    | BuildingCapabilityFlags.ExtractsFromNode,
                    storageCapacity: 40,
                    workerSlots: 2)),
            new(
                StoneMineId,
                code: "stone_mine",
                displayName: "Stone Mine",
                BuildingCategory.Extraction,
                BuildingCapabilityFlags.ProvidesWorkplace
                    | BuildingCapabilityFlags.ProducesResources
                    | BuildingCapabilityFlags.ExtractsFromNode
                    | BuildingCapabilityFlags.SupportsNpcInteraction
                    | BuildingCapabilityFlags.SupportsPlayerInteraction
                    | BuildingCapabilityFlags.BlocksPathing,
                new[]
                {
                    new ResourceAmount(ResourceCatalog.WoodId, 10),
                    new ResourceAmount(ResourceCatalog.StoneId, 8)
                },
                new int2(5, 5),
                buildWorkRequired: 110f,
                ExtractionInteractions(),
                new BuildingNpcProfileDefinition(supportsWorkers: true, workerSlots: 2),
                new BuildingOperationDefinition(
                    BuildingCapabilityFlags.ProvidesWorkplace
                    | BuildingCapabilityFlags.ProducesResources
                    | BuildingCapabilityFlags.ExtractsFromNode,
                    storageCapacity: 40,
                    workerSlots: 2)),
            new(
                WorkbenchId,
                code: "workbench",
                displayName: "Workbench",
                BuildingCategory.Production,
                BuildingCapabilityFlags.ProvidesWorkplace
                    | BuildingCapabilityFlags.ProducesResources
                    | BuildingCapabilityFlags.ConsumesResources
                    | BuildingCapabilityFlags.OpensQueue
                    | BuildingCapabilityFlags.SupportsNpcInteraction
                    | BuildingCapabilityFlags.SupportsPlayerInteraction
                    | BuildingCapabilityFlags.BlocksPathing,
                new[]
                {
                    new ResourceAmount(ResourceCatalog.WoodId, 14),
                    new ResourceAmount(ResourceCatalog.StoneId, 6)
                },
                new int2(4, 4),
                buildWorkRequired: 120f,
                new[]
                {
                    new BuildingInteractionDefinition(BuildingInteractionKind.OpenDetails, requiresCompletedBuilding: false),
                    new BuildingInteractionDefinition(BuildingInteractionKind.DepositConstructionResources, requiresCompletedBuilding: false),
                    new BuildingInteractionDefinition(BuildingInteractionKind.ContributeBuildWork, requiresCompletedBuilding: false),
                    new BuildingInteractionDefinition(BuildingInteractionKind.AssignWorker, requiresCompletedBuilding: true),
                    new BuildingInteractionDefinition(BuildingInteractionKind.OpenProductionQueue, requiresCompletedBuilding: true),
                    new BuildingInteractionDefinition(BuildingInteractionKind.SetRecipe, requiresCompletedBuilding: true),
                    new BuildingInteractionDefinition(BuildingInteractionKind.ClaimOutput, requiresCompletedBuilding: true)
                },
                new BuildingNpcProfileDefinition(supportsWorkers: true, workerSlots: 2),
                new BuildingOperationDefinition(
                    BuildingCapabilityFlags.ProvidesWorkplace
                    | BuildingCapabilityFlags.ProducesResources
                    | BuildingCapabilityFlags.ConsumesResources
                    | BuildingCapabilityFlags.OpensQueue,
                    storageCapacity: 24,
                    workerSlots: 2))
        };

        static BuildingCatalogData()
        {
            BuildingCatalogValidator.Validate(Definitions);
        }

        public static IReadOnlyList<BuildingDefinition> All => Definitions;

        public static BuildingDefinition Get(BuildingId id)
        {
            if (TryGet(id, out var definition))
                return definition;

            throw new InvalidOperationException($"Missing {nameof(BuildingDefinition)} for building id {id.Value} in {nameof(BuildingCatalogData)}.");
        }

        public static bool TryGet(BuildingId id, out BuildingDefinition definition)
        {
            for (var i = 0; i < Definitions.Length; i++)
            {
                if (Definitions[i].Id != id)
                    continue;

                definition = Definitions[i];
                return true;
            }

            definition = default;
            return false;
        }

        private static BuildingInteractionDefinition[] ConstructionInteractions()
        {
            return new[]
            {
                new BuildingInteractionDefinition(BuildingInteractionKind.OpenDetails, requiresCompletedBuilding: false),
                new BuildingInteractionDefinition(BuildingInteractionKind.DepositConstructionResources, requiresCompletedBuilding: false),
                new BuildingInteractionDefinition(BuildingInteractionKind.ContributeBuildWork, requiresCompletedBuilding: false)
            };
        }

        private static BuildingInteractionDefinition[] ExtractionInteractions()
        {
            return new[]
            {
                new BuildingInteractionDefinition(BuildingInteractionKind.OpenDetails, requiresCompletedBuilding: false),
                new BuildingInteractionDefinition(BuildingInteractionKind.DepositConstructionResources, requiresCompletedBuilding: false),
                new BuildingInteractionDefinition(BuildingInteractionKind.ContributeBuildWork, requiresCompletedBuilding: false),
                new BuildingInteractionDefinition(BuildingInteractionKind.AssignWorker, requiresCompletedBuilding: true),
                new BuildingInteractionDefinition(BuildingInteractionKind.Extract, requiresCompletedBuilding: true)
            };
        }
    }
}
