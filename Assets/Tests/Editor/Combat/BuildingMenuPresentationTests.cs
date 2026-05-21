using NUnit.Framework;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;

namespace StaticMlp.Tests.Combat
{
    public sealed class BuildingMenuPresentationTests
    {
        [Test]
        public void Create_BuildsCardForEveryStage1CatalogEntry()
        {
            var state = new BuildingMenuState
            {
                IsOpen = true,
            };

            var presentation = BuildingMenuPresentation.Create(in state);

            Assert.That(presentation.IsOpen, Is.True);
            Assert.That(presentation.SelectedBuildingName, Is.EqualTo("Select a building"));
            Assert.That(presentation.Cards.Length, Is.EqualTo(BuildingCatalogData.All.Count));

            for (var i = 0; i < BuildingCatalogData.All.Count; i++)
            {
                var definition = BuildingCatalogData.All[i];
                var card = presentation.Cards[i];

                Assert.That(card.BuildingId, Is.EqualTo(definition.Id));
                Assert.That(card.DisplayName, Is.EqualTo(BuildingPresentationCatalog.Get(definition.Id).DisplayName));
                Assert.That(card.Category, Is.EqualTo(definition.Category));
                Assert.That(card.CategoryLabel, Is.Not.Empty);
                Assert.That(card.CostLabel, Does.Contain("Wood"));
                Assert.That(card.IsSelected, Is.False);
            }
        }

        [Test]
        public void Create_BuildsCategorySummaryFromCatalog()
        {
            var state = new BuildingMenuState
            {
                IsOpen = true,
            };

            var presentation = BuildingMenuPresentation.Create(in state);

            Assert.That(presentation.Categories.Length, Is.EqualTo(5));
            AssertCategory(presentation, BuildingCategory.Service, "Service", 1, isSelected: false);
            AssertCategory(presentation, BuildingCategory.Logistics, "Logistics", 1, isSelected: false);
            AssertCategory(presentation, BuildingCategory.Housing, "Housing", 1, isSelected: false);
            AssertCategory(presentation, BuildingCategory.Extraction, "Extraction", 2, isSelected: false);
            AssertCategory(presentation, BuildingCategory.Production, "Production", 1, isSelected: false);
        }

        [Test]
        public void Create_MarksSelectedCardAndCategory()
        {
            var state = new BuildingMenuState
            {
                IsOpen = true,
            };
            state.Select(BuildingCatalogData.WorkbenchId);

            var presentation = BuildingMenuPresentation.Create(in state);

            Assert.That(presentation.SelectedBuildingName, Is.EqualTo("Workbench"));
            AssertSelectedCard(presentation, BuildingCatalogData.WorkbenchId);
            AssertCategory(presentation, BuildingCategory.Production, "Production", 1, isSelected: true);
        }

        [Test]
        public void State_SelectAndClear_OnlyTracksOpenStateAndBuildingId()
        {
            var state = new BuildingMenuState
            {
                IsOpen = true,
            };

            state.Select(BuildingCatalogData.StockpileId);

            Assert.That(state.IsOpen, Is.False);
            Assert.That(state.HasSelection, Is.True);
            Assert.That(state.SelectedBuildingId, Is.EqualTo(BuildingCatalogData.StockpileId));

            state.ClearSelection();

            Assert.That(state.IsOpen, Is.False);
            Assert.That(state.HasSelection, Is.False);
            Assert.That(state.SelectedBuildingId, Is.EqualTo(default(BuildingId)));
        }

        private static void AssertCategory(
            BuildingMenuPresentation presentation,
            BuildingCategory category,
            string label,
            int cardCount,
            bool isSelected)
        {
            for (var i = 0; i < presentation.Categories.Length; i++)
            {
                var item = presentation.Categories[i];
                if (item.Category != category)
                    continue;

                Assert.That(item.Label, Is.EqualTo(label));
                Assert.That(item.CardCount, Is.EqualTo(cardCount));
                Assert.That(item.IsSelected, Is.EqualTo(isSelected));
                return;
            }

            Assert.Fail($"Missing category {category}.");
        }

        private static void AssertSelectedCard(BuildingMenuPresentation presentation, BuildingId buildingId)
        {
            var found = false;

            for (var i = 0; i < presentation.Cards.Length; i++)
            {
                var card = presentation.Cards[i];
                if (card.BuildingId == buildingId)
                {
                    Assert.That(card.IsSelected, Is.True);
                    found = true;
                    continue;
                }

                Assert.That(card.IsSelected, Is.False);
            }

            Assert.That(found, Is.True);
        }
    }
}
