using System.Collections.Generic;
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

        public static BuildingMenuPresentation Create(
            in BuildingMenuState state,
            ISettlementUnlockReadModel unlockReadModel)
        {
            return Create(in state, BuildingCatalogData.All, unlockReadModel);
        }

        public static BuildingMenuPresentation Create(
            in BuildingMenuState state,
            IReadOnlyList<BuildingDefinition> definitions,
            ISettlementUnlockReadModel unlockReadModel)
        {
            var selectedCategory = BuildingCategory.None;
            var selectedName = "Select a building";

            if (state.HasSelection)
            {
                var selectedDefinition = GetDefinition(definitions, state.SelectedBuildingId);
                selectedCategory = selectedDefinition.Category;
                selectedName = BuildingPresentationCatalog.Get(selectedDefinition.Id).DisplayName;
            }

            return new BuildingMenuPresentation(
                state.IsOpen,
                selectedName,
                BuildCategories(definitions, selectedCategory),
                BuildCards(definitions, state.SelectedBuildingId, unlockReadModel));
        }

        private static BuildingMenuCategoryPresentation[] BuildCategories(
            IReadOnlyList<BuildingDefinition> definitions,
            BuildingCategory selectedCategory)
        {
            var categories = new BuildingMenuCategoryPresentation[CountCategories(definitions)];
            var writeIndex = 0;

            for (var i = 0; i < definitions.Count; i++)
            {
                var category = definitions[i].Category;
                if (ContainsCategory(categories, writeIndex, category))
                    continue;

                categories[writeIndex++] = new BuildingMenuCategoryPresentation(
                    category,
                    definitions[i].CategoryDisplayName,
                    CountCards(definitions, category),
                    selectedCategory == category);
            }

            return categories;
        }

        private static BuildingMenuCardPresentation[] BuildCards(
            IReadOnlyList<BuildingDefinition> definitions,
            BuildingId selectedBuildingId,
            ISettlementUnlockReadModel unlockReadModel)
        {
            var cards = new BuildingMenuCardPresentation[definitions.Count];

            for (var i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                var presentation = BuildingPresentationCatalog.Get(definition.Id);
                var isAvailable = UnlockEvaluation.IsMet(in definition.UnlockRequirement, unlockReadModel);
                cards[i] = new BuildingMenuCardPresentation(
                    definition.Id,
                    definition.Category,
                    presentation.DisplayName,
                    definition.CategoryDisplayName,
                    FormatConstructionCost(definition.ConstructionCost),
                    selectedBuildingId == definition.Id,
                    isAvailable,
                    isAvailable ? string.Empty : FormatLockedReason(in definition.UnlockRequirement));
            }

            return cards;
        }

        private static int CountCategories(IReadOnlyList<BuildingDefinition> definitions)
        {
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

        private static int CountCards(IReadOnlyList<BuildingDefinition> definitions, BuildingCategory category)
        {
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

        private static BuildingDefinition GetDefinition(
            IReadOnlyList<BuildingDefinition> definitions,
            BuildingId buildingId)
        {
            for (var i = 0; i < definitions.Count; i++)
            {
                if (definitions[i].Id == buildingId)
                    return definitions[i];
            }

            throw new System.InvalidOperationException($"Missing building definition {buildingId.Value}.");
        }

        private static string FormatLockedReason(in UnlockRequirement requirement)
        {
            switch (requirement.Kind)
            {
                case UnlockRequirementKind.SettlementLevel:
                    return $"Requires settlement level {requirement.IntParameter}.";

                case UnlockRequirementKind.BuildingConstructed:
                    return $"Requires {BuildingPresentationCatalog.Get(new BuildingId((ushort)requirement.IntParameter)).DisplayName}.";

                case UnlockRequirementKind.None:
                    return string.Empty;

                default:
                    throw new System.InvalidOperationException($"Unknown unlock requirement kind {requirement.Kind}.");
            }
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

    }
}
