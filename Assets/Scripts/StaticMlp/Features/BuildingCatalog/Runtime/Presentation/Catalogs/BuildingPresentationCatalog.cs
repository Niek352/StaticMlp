using System.Collections.Generic;

namespace StaticMlp.Features.BuildingCatalog
{
    public static class BuildingPresentationCatalog
    {
        private static readonly BuildingPresentationDefinition[] Definitions =
        {
            new(
                BuildingCatalogData.CampCoreId,
                displayName: "Camp Core",
                ghostPreviewViewPath: "Views/Buildings/WoodenHutGhostPreview",
                blueprintViewPath: "Views/Buildings/WoodenHutBlueprint",
                finishedViewPath: "Views/Buildings/WoodenHut"),
            new(
                BuildingCatalogData.StockpileId,
                displayName: "Stockpile",
                ghostPreviewViewPath: "Views/Buildings/WoodenHutGhostPreview",
                blueprintViewPath: "Views/Buildings/WoodenHutBlueprint",
                finishedViewPath: "Views/Buildings/WoodenHut"),
            new(
                BuildingCatalogData.BedrollShelterId,
                displayName: "Bedroll Shelter",
                ghostPreviewViewPath: "Views/Buildings/WoodenHutGhostPreview",
                blueprintViewPath: "Views/Buildings/WoodenHutBlueprint",
                finishedViewPath: "Views/Buildings/WoodenHut"),
            new(
                BuildingCatalogData.LumberCampId,
                displayName: "Lumber Camp",
                ghostPreviewViewPath: "Views/Buildings/WoodenHutGhostPreview",
                blueprintViewPath: "Views/Buildings/WoodenHutBlueprint",
                finishedViewPath: "Views/Buildings/WoodenHut"),
            new(
                BuildingCatalogData.StoneMineId,
                displayName: "Stone Mine",
                ghostPreviewViewPath: "Views/Buildings/WoodenHutGhostPreview",
                blueprintViewPath: "Views/Buildings/WoodenHutBlueprint",
                finishedViewPath: "Views/Buildings/WoodenHut"),
            new(
                BuildingCatalogData.WorkbenchId,
                displayName: "Workbench",
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
