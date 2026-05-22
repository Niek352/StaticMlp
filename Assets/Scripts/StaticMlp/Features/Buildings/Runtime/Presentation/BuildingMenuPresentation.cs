using System.Text;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Buildings
{
    public readonly struct BuildingMenuPresentation
    {
        public readonly bool IsOpen;
        public readonly string SelectedBuildingName;
        public readonly BuildingMenuCategoryPresentation[] Categories;
        public readonly BuildingMenuCardPresentation[] Cards;

        public BuildingMenuPresentation(
            bool isOpen,
            string selectedBuildingName,
            BuildingMenuCategoryPresentation[] categories,
            BuildingMenuCardPresentation[] cards)
        {
            IsOpen = isOpen;
            SelectedBuildingName = selectedBuildingName;
            Categories = categories;
            Cards = cards;
        }

        public static BuildingMenuPresentation Create(in BuildingMenuState state)
        {
            var selectedCategory = BuildingCategory.None;
            var selectedName = "Select a building";

            if (state.HasSelection)
            {
                var selectedDefinition = BuildingCatalogData.Get(state.SelectedBuildingId);
                selectedCategory = selectedDefinition.Category;
                selectedName = BuildingPresentationCatalog.Get(selectedDefinition.Id).DisplayName;
            }

            return new BuildingMenuPresentation(
                state.IsOpen,
                selectedName,
                BuildCategories(selectedCategory),
                BuildCards(state.SelectedBuildingId));
        }

        private static BuildingMenuCategoryPresentation[] BuildCategories(BuildingCategory selectedCategory)
        {
            var definitions = BuildingCatalogData.All;
            var categories = new BuildingMenuCategoryPresentation[CountCategories()];
            var writeIndex = 0;

            for (var i = 0; i < definitions.Count; i++)
            {
                var category = definitions[i].Category;
                if (ContainsCategory(categories, writeIndex, category))
                    continue;

                categories[writeIndex++] = new BuildingMenuCategoryPresentation(
                    category,
                    LabelForCategory(category),
                    CountCards(category),
                    selectedCategory == category);
            }

            return categories;
        }

        private static BuildingMenuCardPresentation[] BuildCards(BuildingId selectedBuildingId)
        {
            var definitions = BuildingCatalogData.All;
            var cards = new BuildingMenuCardPresentation[definitions.Count];

            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                var presentation = BuildingPresentationCatalog.Get(definition.Id);
                cards[i] = new BuildingMenuCardPresentation(
                    definition.Id,
                    definition.Category,
                    presentation.DisplayName,
                    LabelForCategory(definition.Category),
                    FormatConstructionCost(definition.ConstructionCost),
                    selectedBuildingId == definition.Id);
            }

            return cards;
        }

        private static int CountCategories()
        {
            var definitions = BuildingCatalogData.All;
            var count = 0;

            for (var i = 0; i < definitions.Count; i++)
            {
                var category = definitions[i].Category;
                var exists = false;

                for (var j = 0; j < i; j++)
                {
                    if (definitions[j].Category != category)
                        continue;

                    exists = true;
                    break;
                }

                if (!exists)
                    count++;
            }

            return count;
        }

        private static int CountCards(BuildingCategory category)
        {
            var definitions = BuildingCatalogData.All;
            var count = 0;

            for (var i = 0; i < definitions.Count; i++)
            {
                if (definitions[i].Category == category)
                    count++;
            }

            return count;
        }

        private static bool ContainsCategory(
            BuildingMenuCategoryPresentation[] categories,
            int length,
            BuildingCategory category)
        {
            for (var i = 0; i < length; i++)
            {
                if (categories[i].Category == category)
                    return true;
            }

            return false;
        }

        private static string FormatConstructionCost(ResourceAmount[] constructionCost)
        {
            var builder = new StringBuilder();

            for (var i = 0; i < constructionCost.Length; i++)
            {
                if (i > 0)
                    builder.Append("  ");

                builder.Append(ResourceCatalog.Get(constructionCost[i].Id).DisplayName);
                builder.Append(' ');
                builder.Append(constructionCost[i].Amount);
            }

            return builder.ToString();
        }

        private static string LabelForCategory(BuildingCategory category)
        {
            switch (category)
            {
                case BuildingCategory.Housing:
                    return "Housing";
                case BuildingCategory.Logistics:
                    return "Logistics";
                case BuildingCategory.Extraction:
                    return "Extraction";
                case BuildingCategory.Production:
                    return "Production";
                case BuildingCategory.Service:
                    return "Service";
                case BuildingCategory.Defense:
                    return "Defense";
                case BuildingCategory.Research:
                    return "Research";
                default:
                    return "Uncategorized";
            }
        }

    }
}
