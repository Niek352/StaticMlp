using System.Collections.Generic;

namespace StaticMlp.Features.BuildingCatalog
{
    public static class BuildingPresentationCatalog
    {
        private static readonly BuildingPresentationDefinition[] Definitions =
        {
            new(
                BuildingCatalog.WoodenHutId,
                ghostPreviewViewPath: "Views/Buildings/WoodenHutGhostPreview",
                blueprintViewPath: "Views/Buildings/WoodenHutBlueprint",
                finishedViewPath: "Views/Buildings/WoodenHut")
        };

        public static IReadOnlyList<BuildingPresentationDefinition> All => Definitions;

        public static bool TryGetDefinition(BuildingId id, out BuildingPresentationDefinition definition)
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
