using System;
using System.Collections.Generic;

namespace StaticMlp.Features.Settlement
{
    public static class ResourceCatalog
    {
        public static readonly ResourceId WoodId = new(1);
        public static readonly ResourceId StoneId = new(2);

        private static readonly ResourceDefinition[] Definitions =
        {
            new(
                WoodId,
                ResourceUsageFlags.Construction | ResourceUsageFlags.ExpeditionReward,
                isSettlementStored: true,
                startingSettlementAmount: 50),
            new(
                StoneId,
                ResourceUsageFlags.Construction | ResourceUsageFlags.ExpeditionReward,
                isSettlementStored: true,
                startingSettlementAmount: 25)
        };

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
