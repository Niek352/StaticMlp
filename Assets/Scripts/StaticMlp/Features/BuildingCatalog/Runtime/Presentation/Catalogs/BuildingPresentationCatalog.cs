using System.Collections.Generic;

namespace StaticMlp.Features.BuildingCatalog
{
    public static class BuildingPresentationCatalog
    {
        private static readonly BuildingPresentationDefinition[] Definitions =
        {
            new(
                BuildingCatalogData.WoodenHutId,
                displayName: "Wooden Hut",
                ghostPreviewViewPath: "Views/Buildings/WoodenHutGhostPreview",
                blueprintViewPath: "Views/Buildings/WoodenHutBlueprint",
                finishedViewPath: "Views/Buildings/WoodenHut")
        };

        public static IReadOnlyList<BuildingPresentationDefinition> All => Definitions;

        public static BuildingPresentationDefinition Get(BuildingId id)
        {
            if (TryGet(id, out var definition))
                return definition;

            throw new System.InvalidOperationException($"Missing {nameof(BuildingPresentationDefinition)} for building id {id.Value} in {nameof(BuildingPresentationCatalog)}.");
        }

        public static bool TryGet(BuildingId id, out BuildingPresentationDefinition definition)
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
