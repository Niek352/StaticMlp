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
                new[]
                {
                    new ResourceAmount(ResourceCatalog.WoodId, 10),
                    new ResourceAmount(ResourceCatalog.StoneId, 4)
                },
                new int2(4, 5),
                buildWorkRequired: 100f)
        };

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
