using System.Collections.Generic;

namespace StaticMlp.Features.BuildingCatalog
{
    public static class BuildingNetworkCatalog
    {
        private static readonly BuildingNetworkDefinition[] Definitions =
        {
            new(
                BuildingCatalogData.WoodenHutId,
                BuildingNetworkArchetypeIds.WoodenHutBlueprint,
                BuildingNetworkArchetypeIds.WoodenHutFinished)
        };

        public static IReadOnlyList<BuildingNetworkDefinition> All => Definitions;

        public static BuildingNetworkDefinition Get(BuildingId id)
        {
            if (TryGet(id, out var definition))
                return definition;

            throw new System.InvalidOperationException($"Missing {nameof(BuildingNetworkDefinition)} for building id {id.Value} in {nameof(BuildingNetworkCatalog)}.");
        }

        public static bool TryGet(BuildingId id, out BuildingNetworkDefinition definition)
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
