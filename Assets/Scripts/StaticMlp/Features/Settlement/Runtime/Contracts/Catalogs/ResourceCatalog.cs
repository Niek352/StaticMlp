using System;
using System.Collections.Generic;

namespace StaticMlp.Features.Settlement
{
    public static class ResourceCatalog
    {
        public static readonly ResourceId WoodId = new(1);
        public static readonly ResourceId StoneId = new(2);
        public static readonly ResourceId FlowCatalystId = new(3);
        public static readonly ResourceId RefinedPlankId = new(4);
        public static readonly ResourceId ResearchDataId = new(5);
        public static readonly ResourceId StabilityCoreId = new(6);

        private static readonly ResourceDefinition[] Definitions =
        {
            new(
                WoodId,
                ResourceFamily.Raw,
                ResourceUsageFlags.Construction | ResourceUsageFlags.ExpeditionReward,
                isSettlementStored: true,
                startingSettlementAmount: 50),
            new(
                StoneId,
                ResourceFamily.Raw,
                ResourceUsageFlags.Construction | ResourceUsageFlags.ExpeditionReward,
                isSettlementStored: true,
                startingSettlementAmount: 25),
            new(
                FlowCatalystId,
                ResourceFamily.Flow,
                ResourceUsageFlags.None,
                isSettlementStored: false,
                startingSettlementAmount: 0),
            new(
                RefinedPlankId,
                ResourceFamily.Refined,
                ResourceUsageFlags.None,
                isSettlementStored: false,
                startingSettlementAmount: 0),
            new(
                ResearchDataId,
                ResourceFamily.Progression,
                ResourceUsageFlags.None,
                isSettlementStored: false,
                startingSettlementAmount: 0),
            new(
                StabilityCoreId,
                ResourceFamily.Stability,
                ResourceUsageFlags.None,
                isSettlementStored: false,
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
