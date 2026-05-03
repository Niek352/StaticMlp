using System.Collections.Generic;

namespace StaticMlp.Features.BuildingCatalog
{
    public static class BuildingCatalog
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

        public static bool TryGetDefinition(BuildingId id, out BuildingDefinition definition)
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
