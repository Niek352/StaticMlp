using NUnit.Framework;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement;
using Unity.Mathematics;

namespace StaticMlp.Tests.Combat
{
    public sealed class BuildingMenuPresentationTests
    {
        private static readonly ISettlementUnlockReadModel AllUnlocked = new TestUnlockReadModel(10, hasConstructedBuildings: true);

        [Test]
        public void Create_BuildsCardForEveryStage1CatalogEntry()
        {
            var state = new BuildingMenuState
            {
                IsOpen = true,
            };

            var presentation = BuildingMenuPresentation.Create(in state, AllUnlocked);

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
                Assert.That(card.CategoryLabel, Is.EqualTo(definition.CategoryDisplayName));
                Assert.That(card.CostLabel, Does.Contain("Wood"));
                Assert.That(card.IsSelected, Is.False);
                Assert.That(card.IsAvailable, Is.True);
                Assert.That(card.LockedReason, Is.Empty);
            }
        }

        [Test]
        public void Create_BuildsCategorySummaryFromCatalog()
        {
            var state = new BuildingMenuState
            {
                IsOpen = true,
            };

            var presentation = BuildingMenuPresentation.Create(in state, AllUnlocked);

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

            var presentation = BuildingMenuPresentation.Create(in state, AllUnlocked);

            Assert.That(presentation.SelectedBuildingName, Is.EqualTo("Workbench"));
            AssertSelectedCard(presentation, BuildingCatalogData.WorkbenchId);
            AssertCategory(presentation, BuildingCategory.Production, "Production", 1, isSelected: true);
        }

        [Test]
        public void Create_MarksLockedBuildingCardsWithReason()
        {
            var definitions = new[]
            {
                CreateDefinition(BuildingCatalogData.StockpileId, UnlockRequirement.None),
                CreateDefinition(BuildingCatalogData.WorkbenchId, UnlockRequirement.SettlementLevel(2))
            };
            var state = new BuildingMenuState
            {
                IsOpen = true,
            };

            var presentation = BuildingMenuPresentation.Create(
                in state,
                definitions,
                new TestUnlockReadModel(settlementLevel: 1, hasConstructedBuildings: false));

            Assert.That(presentation.Cards[0].IsAvailable, Is.True);
            Assert.That(presentation.Cards[0].LockedReason, Is.Empty);
            Assert.That(presentation.Cards[1].IsAvailable, Is.False);
            Assert.That(presentation.Cards[1].LockedReason, Is.EqualTo("Requires settlement level 2."));
        }

        [Test]
        public void Create_UsesBuildingNameForConstructedBuildingLockReason()
        {
            var definitions = new[]
            {
                CreateDefinition(
                    BuildingCatalogData.WorkbenchId,
                    UnlockRequirement.BuildingConstructed(BuildingCatalogData.StockpileId))
            };
            var state = new BuildingMenuState
            {
                IsOpen = true,
            };

            var presentation = BuildingMenuPresentation.Create(
                in state,
                definitions,
                new TestUnlockReadModel(settlementLevel: 10, hasConstructedBuildings: false));

            Assert.That(presentation.Cards[0].IsAvailable, Is.False);
            Assert.That(presentation.Cards[0].LockedReason, Is.EqualTo("Requires Stockpile."));
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

        private static BuildingDefinition CreateDefinition(
            BuildingId id,
            UnlockRequirement unlockRequirement)
        {
            var definition = BuildingCatalogData.Get(id);
            return new BuildingDefinition(
                definition.Id,
                definition.Code,
                definition.DisplayName,
                definition.Category,
                definition.CategoryDisplayName,
                definition.Capabilities,
                new[] { new ResourceAmount(ResourceCatalog.WoodId, 1) },
                new int2(1, 1),
                definition.BuildWorkRequired,
                definition.Interactions,
                definition.NpcProfile,
                definition.Operation,
                unlockRequirement);
        }

        private sealed class TestUnlockReadModel : ISettlementUnlockReadModel
        {
            private readonly bool _hasConstructedBuildings;

            public TestUnlockReadModel(int settlementLevel, bool hasConstructedBuildings)
            {
                SettlementLevel = settlementLevel;
                _hasConstructedBuildings = hasConstructedBuildings;
            }

            public int SettlementLevel { get; }

            public bool HasConstructedBuilding(int buildingIdValue)
            {
                return _hasConstructedBuildings;
            }
        }
    }
}
