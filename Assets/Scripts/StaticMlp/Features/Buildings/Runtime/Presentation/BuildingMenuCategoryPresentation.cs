using StaticMlp.Features.BuildingCatalog;

namespace StaticMlp.Features.Buildings
{
    public readonly struct BuildingMenuCategoryPresentation
    {
        public readonly BuildingCategory Category;
        public readonly string Label;
        public readonly int CardCount;
        public readonly bool IsSelected;

        public BuildingMenuCategoryPresentation(
            BuildingCategory category,
            string label,
            int cardCount,
            bool isSelected)
        {
            Category = category;
            Label = label;
            CardCount = cardCount;
            IsSelected = isSelected;
        }
    }
}
