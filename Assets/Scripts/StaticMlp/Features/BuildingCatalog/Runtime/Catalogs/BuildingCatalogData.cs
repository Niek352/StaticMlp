using System;
using System.Collections.Generic;
using StaticMlp.Features.Settlement;
using Unity.Mathematics;

namespace StaticMlp.Features.BuildingCatalog
{
    public static class BuildingCatalogData
    {
        public static readonly BuildingId WoodenHutId = new(1);

        private static readonly BuildingDefinition[] Definitions =
        {
            new(
                WoodenHutId,
                code: "wooden_hut",
                displayName: "Wooden Hut",
                BuildingCategory.Housing,
                BuildingCapabilityFlags.ProvidesHousing
                    | BuildingCapabilityFlags.SupportsPlayerInteraction
                    | BuildingCapabilityFlags.BlocksPathing,
                new[]
                {
                    new ResourceAmount(ResourceCatalog.WoodId, 10),
                    new ResourceAmount(ResourceCatalog.StoneId, 4)
                },
                new int2(4, 5),
                buildWorkRequired: 100f,
                new[]
                {
                    new BuildingInteractionDefinition(BuildingInteractionKind.OpenDetails, requiresCompletedBuilding: false),
                    new BuildingInteractionDefinition(BuildingInteractionKind.DepositConstructionResources, requiresCompletedBuilding: false),
                    new BuildingInteractionDefinition(BuildingInteractionKind.ContributeBuildWork, requiresCompletedBuilding: false)
                },
                BuildingNpcProfileDefinition.None,
                BuildingOperationDefinition.None)
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
    }
}
