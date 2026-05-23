using System;
using NUnit.Framework;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Settlement;
using Unity.Mathematics;

namespace StaticMlp.Tests.Settlement
{
    public sealed class BuildingCatalogValidatorTests
    {
        [Test]
        public void Validate_CurrentCatalog_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => BuildingCatalogValidator.Validate(BuildingCatalogData.All));

            var definition = BuildingCatalogData.Get(BuildingCatalogData.CampCoreId);
            Assert.That(definition.Code, Is.EqualTo("camp_core"));
            Assert.That(definition.DisplayName, Is.EqualTo("Camp Core"));
            Assert.That(definition.Category, Is.EqualTo(BuildingCategory.Service));
            Assert.That(definition.CategoryDisplayName, Is.EqualTo("Service"));
            Assert.That(definition.Capabilities.HasFlag(BuildingCapabilityFlags.SupportsPlayerInteraction), Is.True);
            Assert.That(definition.Interactions.Length, Is.EqualTo(3));
            Assert.That(definition.Interactions[0].DisplayName, Is.EqualTo("Open"));
        }

        [Test]
        public void Validate_CurrentCatalog_HasStage1BuildingSetExactlyOnce()
        {
            Assert.That(BuildingCatalogData.All.Count, Is.EqualTo(6));

            AssertCatalogContainsOnce(BuildingCatalogData.CampCoreId, "camp_core");
            AssertCatalogContainsOnce(BuildingCatalogData.StockpileId, "stockpile");
            AssertCatalogContainsOnce(BuildingCatalogData.BedrollShelterId, "bedroll_shelter");
            AssertCatalogContainsOnce(BuildingCatalogData.LumberCampId, "lumber_camp");
            AssertCatalogContainsOnce(BuildingCatalogData.StoneMineId, "stone_mine");
            AssertCatalogContainsOnce(BuildingCatalogData.WorkbenchId, "workbench");
        }

        [Test]
        public void NetworkCatalog_HasEntriesForEveryStage1Building()
        {
            Assert.That(BuildingNetworkCatalog.All.Count, Is.EqualTo(BuildingCatalogData.All.Count));

            for (var i = 0; i < BuildingCatalogData.All.Count; i++)
            {
                var building = BuildingCatalogData.All[i];
                var network = BuildingNetworkCatalog.Get(building.Id);

                Assert.That(network.BlueprintArchetypeId, Is.Not.EqualTo(0));
                Assert.That(network.FinishedArchetypeId, Is.Not.EqualTo(0));
                Assert.That(network.FinishedArchetypeId, Is.Not.EqualTo(network.BlueprintArchetypeId));
            }
        }

        [Test]
        public void PresentationCatalog_HasEntriesForEveryStage1Building()
        {
            Assert.That(BuildingPresentationCatalog.All.Count, Is.EqualTo(BuildingCatalogData.All.Count));

            for (var i = 0; i < BuildingCatalogData.All.Count; i++)
            {
                var building = BuildingCatalogData.All[i];
                var presentation = BuildingPresentationCatalog.Get(building.Id);

                Assert.That(presentation.DisplayName, Is.EqualTo(building.DisplayName));
                Assert.That(presentation.GhostPreviewViewPath, Is.Not.Empty);
                Assert.That(presentation.BlueprintViewPath, Is.Not.Empty);
                Assert.That(presentation.FinishedViewPath, Is.Not.Empty);
            }
        }

        [Test]
        public void Validate_DuplicateBuildingIds_Throws()
        {
            var definitions = new[]
            {
                CreateDefinition(BuildingCatalogData.CampCoreId),
                CreateDefinition(BuildingCatalogData.CampCoreId)
            };

            Assert.Throws<InvalidOperationException>(() => BuildingCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_MissingDisplayName_Throws()
        {
            var definitions = new[]
            {
                new BuildingDefinition(
                    new BuildingId(20),
                    code: "missing_display",
                    displayName: "",
                    BuildingCategory.Service,
                    categoryDisplayName: "Service",
                    BuildingCapabilityFlags.SupportsPlayerInteraction,
                    new[]
                    {
                        new ResourceAmount(ResourceCatalog.WoodId, 1)
                    },
                    new int2(1, 1),
                    buildWorkRequired: 1f,
                    new[]
                    {
                        new BuildingInteractionDefinition(BuildingInteractionKind.OpenDetails, displayName: "Open", requiresCompletedBuilding: false)
                    },
                    BuildingNpcProfileDefinition.None,
                    BuildingOperationDefinition.None)
            };

            Assert.Throws<InvalidOperationException>(() => BuildingCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_MissingCategoryDisplayName_Throws()
        {
            var definitions = new[]
            {
                new BuildingDefinition(
                    new BuildingId(25),
                    code: "missing_category_display",
                    displayName: "Missing Category Display",
                    BuildingCategory.Service,
                    categoryDisplayName: "",
                    BuildingCapabilityFlags.SupportsPlayerInteraction,
                    new[]
                    {
                        new ResourceAmount(ResourceCatalog.WoodId, 1)
                    },
                    new int2(1, 1),
                    buildWorkRequired: 1f,
                    new[]
                    {
                        new BuildingInteractionDefinition(BuildingInteractionKind.OpenDetails, displayName: "Open", requiresCompletedBuilding: false)
                    },
                    BuildingNpcProfileDefinition.None,
                    BuildingOperationDefinition.None)
            };

            Assert.Throws<InvalidOperationException>(() => BuildingCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_InconsistentCategoryDisplayName_Throws()
        {
            var first = CreateDefinition(new BuildingId(26));
            var second = new BuildingDefinition(
                new BuildingId(27),
                code: "building_27",
                displayName: "Building 27",
                BuildingCategory.Housing,
                categoryDisplayName: "Shelters",
                BuildingCapabilityFlags.SupportsPlayerInteraction,
                new[]
                {
                    new ResourceAmount(ResourceCatalog.WoodId, 1)
                },
                new int2(1, 1),
                buildWorkRequired: 1f,
                new[]
                {
                    new BuildingInteractionDefinition(BuildingInteractionKind.OpenDetails, displayName: "Open", requiresCompletedBuilding: false)
                },
                BuildingNpcProfileDefinition.None,
                BuildingOperationDefinition.None);

            Assert.Throws<InvalidOperationException>(() => BuildingCatalogValidator.Validate(new[] { first, second }));
        }

        [Test]
        public void Validate_MissingInteractionDisplayName_Throws()
        {
            var definitions = new[]
            {
                new BuildingDefinition(
                    new BuildingId(28),
                    code: "missing_interaction_display",
                    displayName: "Missing Interaction Display",
                    BuildingCategory.Service,
                    categoryDisplayName: "Service",
                    BuildingCapabilityFlags.SupportsPlayerInteraction,
                    new[]
                    {
                        new ResourceAmount(ResourceCatalog.WoodId, 1)
                    },
                    new int2(1, 1),
                    buildWorkRequired: 1f,
                    new[]
                    {
                        new BuildingInteractionDefinition(BuildingInteractionKind.OpenDetails, displayName: "", requiresCompletedBuilding: false)
                    },
                    BuildingNpcProfileDefinition.None,
                    BuildingOperationDefinition.None)
            };

            Assert.Throws<InvalidOperationException>(() => BuildingCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_StorageCapabilityWithoutOperationProfile_Throws()
        {
            var definitions = new[]
            {
                new BuildingDefinition(
                    new BuildingId(21),
                    code: "storage_without_operation",
                    displayName: "Storage Without Operation",
                    BuildingCategory.Logistics,
                    categoryDisplayName: "Logistics",
                    BuildingCapabilityFlags.ProvidesStorage,
                    new[]
                    {
                        new ResourceAmount(ResourceCatalog.WoodId, 1)
                    },
                    new int2(1, 1),
                    buildWorkRequired: 1f,
                    Array.Empty<BuildingInteractionDefinition>(),
                    BuildingNpcProfileDefinition.None,
                    BuildingOperationDefinition.None)
            };

            Assert.Throws<InvalidOperationException>(() => BuildingCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_ExtractionBuildingWithoutOutputResource_Throws()
        {
            var definitions = new[]
            {
                CreateExtractionDefinition(
                    new BuildingId(22),
                    new BuildingOperationDefinition(
                        BuildingCapabilityFlags.ProvidesWorkplace
                        | BuildingCapabilityFlags.ProducesResources
                        | BuildingCapabilityFlags.ExtractsFromNode,
                        storageCapacity: 10,
                        workerSlots: 1))
            };

            Assert.Throws<InvalidOperationException>(() => BuildingCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_ExtractionBuildingWithMissingOutputResource_Throws()
        {
            var definitions = new[]
            {
                CreateExtractionDefinition(
                    new BuildingId(23),
                    new BuildingOperationDefinition(
                        BuildingCapabilityFlags.ProvidesWorkplace
                        | BuildingCapabilityFlags.ProducesResources
                        | BuildingCapabilityFlags.ExtractsFromNode,
                        storageCapacity: 10,
                        workerSlots: 1,
                        outputResourceId: new ResourceId(ushort.MaxValue)))
            };

            Assert.Throws<InvalidOperationException>(() => BuildingCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_ExtractionBuildingWithNonProductionOutputResource_Throws()
        {
            var definitions = new[]
            {
                CreateExtractionDefinition(
                    new BuildingId(24),
                    new BuildingOperationDefinition(
                        BuildingCapabilityFlags.ProvidesWorkplace
                        | BuildingCapabilityFlags.ProducesResources
                        | BuildingCapabilityFlags.ExtractsFromNode,
                        storageCapacity: 10,
                        workerSlots: 1,
                        outputResourceId: ResourceCatalog.ResearchDataId))
            };

            Assert.Throws<InvalidOperationException>(() => BuildingCatalogValidator.Validate(definitions));
        }

        private static BuildingDefinition CreateDefinition(BuildingId id)
        {
            return new BuildingDefinition(
                id,
                code: $"building_{id.Value}",
                displayName: $"Building {id.Value}",
                BuildingCategory.Housing,
                categoryDisplayName: "Housing",
                BuildingCapabilityFlags.SupportsPlayerInteraction,
                new[]
                {
                    new ResourceAmount(ResourceCatalog.WoodId, 1)
                },
                new int2(1, 1),
                buildWorkRequired: 1f,
                new[]
                {
                    new BuildingInteractionDefinition(BuildingInteractionKind.OpenDetails, displayName: "Open", requiresCompletedBuilding: false)
                },
                BuildingNpcProfileDefinition.None,
                BuildingOperationDefinition.None);
        }

        private static BuildingDefinition CreateExtractionDefinition(
            BuildingId id,
            BuildingOperationDefinition operation)
        {
            return new BuildingDefinition(
                id,
                code: $"extraction_{id.Value}",
                displayName: $"Extraction {id.Value}",
                BuildingCategory.Extraction,
                categoryDisplayName: "Extraction",
                BuildingCapabilityFlags.ProvidesWorkplace
                | BuildingCapabilityFlags.ProducesResources
                | BuildingCapabilityFlags.ExtractsFromNode,
                new[]
                {
                    new ResourceAmount(ResourceCatalog.WoodId, 1)
                },
                new int2(1, 1),
                buildWorkRequired: 1f,
                Array.Empty<BuildingInteractionDefinition>(),
                BuildingNpcProfileDefinition.None,
                operation);
        }

        private static void AssertCatalogContainsOnce(BuildingId id, string code)
        {
            var count = 0;
            for (var i = 0; i < BuildingCatalogData.All.Count; i++)
            {
                if (BuildingCatalogData.All[i].Id != id)
                    continue;

                count++;
                Assert.That(BuildingCatalogData.All[i].Code, Is.EqualTo(code));
            }

            Assert.That(count, Is.EqualTo(1));
        }
    }
}
