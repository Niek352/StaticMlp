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

            var definition = BuildingCatalogData.Get(BuildingCatalogData.WoodenHutId);
            Assert.That(definition.Code, Is.EqualTo("wooden_hut"));
            Assert.That(definition.DisplayName, Is.EqualTo("Wooden Hut"));
            Assert.That(definition.Category, Is.EqualTo(BuildingCategory.Housing));
            Assert.That(definition.Capabilities.HasFlag(BuildingCapabilityFlags.SupportsPlayerInteraction), Is.True);
            Assert.That(definition.Interactions.Length, Is.EqualTo(3));
        }

        [Test]
        public void Validate_DuplicateBuildingIds_Throws()
        {
            var definitions = new[]
            {
                CreateDefinition(BuildingCatalogData.WoodenHutId),
                CreateDefinition(BuildingCatalogData.WoodenHutId)
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
                    BuildingCapabilityFlags.SupportsPlayerInteraction,
                    new[]
                    {
                        new ResourceAmount(ResourceCatalog.WoodId, 1)
                    },
                    new int2(1, 1),
                    buildWorkRequired: 1f,
                    new[]
                    {
                        new BuildingInteractionDefinition(BuildingInteractionKind.OpenDetails, requiresCompletedBuilding: false)
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

        private static BuildingDefinition CreateDefinition(BuildingId id)
        {
            return new BuildingDefinition(
                id,
                code: $"building_{id.Value}",
                displayName: $"Building {id.Value}",
                BuildingCategory.Housing,
                BuildingCapabilityFlags.SupportsPlayerInteraction,
                new[]
                {
                    new ResourceAmount(ResourceCatalog.WoodId, 1)
                },
                new int2(1, 1),
                buildWorkRequired: 1f,
                new[]
                {
                    new BuildingInteractionDefinition(BuildingInteractionKind.OpenDetails, requiresCompletedBuilding: false)
                },
                BuildingNpcProfileDefinition.None,
                BuildingOperationDefinition.None);
        }
    }
}
