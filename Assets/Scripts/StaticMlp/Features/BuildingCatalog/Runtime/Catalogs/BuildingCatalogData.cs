using System;
using System.Collections.Generic;
using StaticMlp.Features.Settlement;
using Unity.Mathematics;

namespace StaticMlp.Features.BuildingCatalog
{
    public static class BuildingCatalogData
    {
        private const byte OPEN_DETAILS_PRIORITY = 10;
        private const byte ASSIGN_WORKER_PRIORITY = 50;
        private const byte REST_PRIORITY = 60;
        private const byte EXTRACT_PRIORITY = 70;
        private const byte ASSIGN_BED_PRIORITY = 80;
        private const byte STORE_ITEMS_PRIORITY = 90;
        private const byte OPEN_PRODUCTION_QUEUE_PRIORITY = 100;

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
                categoryDisplayName: "Service",
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
                categoryDisplayName: "Logistics",
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
                    new BuildingInteractionDefinition(
                        BuildingInteractionKind.OpenDetails,
                        displayName: "Open",
                        requiresCompletedBuilding: false,
                        OPEN_DETAILS_PRIORITY),
                    new BuildingInteractionDefinition(BuildingInteractionKind.DepositConstructionResources, displayName: "Deposit", requiresCompletedBuilding: false),
                    new BuildingInteractionDefinition(BuildingInteractionKind.ContributeBuildWork, displayName: "Build", requiresCompletedBuilding: false),
                    new BuildingInteractionDefinition(
                        BuildingInteractionKind.StoreItems,
                        displayName: "Store Items",
                        requiresCompletedBuilding: true,
                        STORE_ITEMS_PRIORITY),
                    new BuildingInteractionDefinition(BuildingInteractionKind.WithdrawItems, displayName: "Withdraw", requiresCompletedBuilding: true)
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
                categoryDisplayName: "Housing",
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
                    new BuildingInteractionDefinition(
                        BuildingInteractionKind.OpenDetails,
                        displayName: "Open",
                        requiresCompletedBuilding: false,
                        OPEN_DETAILS_PRIORITY),
                    new BuildingInteractionDefinition(BuildingInteractionKind.DepositConstructionResources, displayName: "Deposit", requiresCompletedBuilding: false),
                    new BuildingInteractionDefinition(BuildingInteractionKind.ContributeBuildWork, displayName: "Build", requiresCompletedBuilding: false),
                    new BuildingInteractionDefinition(
                        BuildingInteractionKind.AssignBed,
                        displayName: "Assign Bed",
                        requiresCompletedBuilding: true,
                        ASSIGN_BED_PRIORITY),
                    new BuildingInteractionDefinition(
                        BuildingInteractionKind.Rest,
                        displayName: "Rest",
                        requiresCompletedBuilding: true,
                        REST_PRIORITY)
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
                categoryDisplayName: "Extraction",
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
                    workerSlots: 2,
                    outputResourceId: ResourceCatalog.WoodId)),
            new(
                StoneMineId,
                code: "stone_mine",
                displayName: "Stone Mine",
                BuildingCategory.Extraction,
                categoryDisplayName: "Extraction",
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
                    workerSlots: 2,
                    outputResourceId: ResourceCatalog.StoneId)),
            new(
                WorkbenchId,
                code: "workbench",
                displayName: "Workbench",
                BuildingCategory.Production,
                categoryDisplayName: "Production",
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
                    new BuildingInteractionDefinition(
                        BuildingInteractionKind.OpenDetails,
                        displayName: "Open",
                        requiresCompletedBuilding: false,
                        OPEN_DETAILS_PRIORITY),
                    new BuildingInteractionDefinition(BuildingInteractionKind.DepositConstructionResources, displayName: "Deposit", requiresCompletedBuilding: false),
                    new BuildingInteractionDefinition(BuildingInteractionKind.ContributeBuildWork, displayName: "Build", requiresCompletedBuilding: false),
                    new BuildingInteractionDefinition(
                        BuildingInteractionKind.AssignWorker,
                        displayName: "Assign Worker",
                        requiresCompletedBuilding: true,
                        ASSIGN_WORKER_PRIORITY),
                    new BuildingInteractionDefinition(
                        BuildingInteractionKind.OpenProductionQueue,
                        displayName: "Open Queue",
                        requiresCompletedBuilding: true,
                        OPEN_PRODUCTION_QUEUE_PRIORITY),
                    new BuildingInteractionDefinition(BuildingInteractionKind.SetRecipe, displayName: "Set Recipe", requiresCompletedBuilding: true),
                    new BuildingInteractionDefinition(BuildingInteractionKind.ClaimOutput, displayName: "Claim Output", requiresCompletedBuilding: true)
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
                new BuildingInteractionDefinition(
                    BuildingInteractionKind.OpenDetails,
                    displayName: "Open",
                    requiresCompletedBuilding: false,
                    OPEN_DETAILS_PRIORITY),
                new BuildingInteractionDefinition(BuildingInteractionKind.DepositConstructionResources, displayName: "Deposit", requiresCompletedBuilding: false),
                new BuildingInteractionDefinition(BuildingInteractionKind.ContributeBuildWork, displayName: "Build", requiresCompletedBuilding: false)
            };
        }

        private static BuildingInteractionDefinition[] ExtractionInteractions()
        {
            return new[]
            {
                new BuildingInteractionDefinition(
                    BuildingInteractionKind.OpenDetails,
                    displayName: "Open",
                    requiresCompletedBuilding: false,
                    OPEN_DETAILS_PRIORITY),
                new BuildingInteractionDefinition(BuildingInteractionKind.DepositConstructionResources, displayName: "Deposit", requiresCompletedBuilding: false),
                new BuildingInteractionDefinition(BuildingInteractionKind.ContributeBuildWork, displayName: "Build", requiresCompletedBuilding: false),
                new BuildingInteractionDefinition(
                    BuildingInteractionKind.AssignWorker,
                    displayName: "Assign Worker",
                    requiresCompletedBuilding: true,
                    ASSIGN_WORKER_PRIORITY),
                new BuildingInteractionDefinition(
                    BuildingInteractionKind.Extract,
                    displayName: "Extract",
                    requiresCompletedBuilding: true,
                    EXTRACT_PRIORITY)
            };
        }
    }
}
