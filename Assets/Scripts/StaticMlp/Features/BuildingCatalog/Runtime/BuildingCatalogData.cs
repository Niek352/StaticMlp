using System;
using System.Collections.Generic;

namespace StaticMlp.Features.BuildingCatalog
{
    public static class BuildingCatalogData
    {
        public static readonly BuildingId WoodenHutId = new(1);

        private static readonly BuildingDefinition[] Definitions =
        {
            new(
                WoodenHutId,
                "Wooden Hut",
                costWood: 10,
                costStone: 4,
                footprintWidth: 4,
                footprintLength: 5,
                buildWorkRequired: 100f)
        };

        public static IReadOnlyList<BuildingDefinition> All => Definitions;

        public static BuildingDefinition GetDefinition(BuildingId id)
        {
            for (var i = 0; i < Definitions.Length; i++)
            {
                if (Definitions[i].Id != id)
                    continue;

                return Definitions[i];
            }

            throw new InvalidOperationException($"Missing {nameof(BuildingDefinition)} for building id {id.Value} in {nameof(BuildingCatalogData)}.");
        }
    }
}
