using StaticMlp.Features.BuildingCatalog;

namespace StaticMlp.Features.Buildings
{
    public readonly struct BuildingMenuCardPresentation
    {
        public readonly BuildingId BuildingId;
        public readonly BuildingCategory Category;
        public readonly string DisplayName;
        public readonly string CategoryLabel;
        public readonly string CostLabel;
        public readonly bool IsSelected;
        public readonly bool IsAvailable;
        public readonly string LockedReason;

        public BuildingMenuCardPresentation(
            BuildingId buildingId,
            BuildingCategory category,
            string displayName,
            string categoryLabel,
            string costLabel,
            bool isSelected,
            bool isAvailable,
            string lockedReason)
        {
            BuildingId = buildingId;
            Category = category;
            DisplayName = displayName;
            CategoryLabel = categoryLabel;
            CostLabel = costLabel;
            IsSelected = isSelected;
            IsAvailable = isAvailable;
            LockedReason = lockedReason;
        }
    }
}
