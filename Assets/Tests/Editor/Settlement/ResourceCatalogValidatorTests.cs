using System;
using System.Collections.Generic;
using NUnit.Framework;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Tests.Settlement
{
    public sealed class ResourceCatalogValidatorTests
    {
        [Test]
        public void Validate_DuplicateResourceIds_Throws()
        {
            var definitions = new[]
            {
                CreateDefinition(new ResourceId(1), ResourceFamily.Raw),
                CreateDefinition(new ResourceId(1), ResourceFamily.Flow)
            };

            Assert.Throws<InvalidOperationException>(() => ResourceCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_MissingFamily_Throws()
        {
            var definitions = new[]
            {
                CreateDefinition(new ResourceId(1), ResourceFamily.None)
            };

            Assert.Throws<InvalidOperationException>(() => ResourceCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_NegativeStoredStartingAmount_Throws()
        {
            var definitions = new[]
            {
                new ResourceDefinition(
                    new ResourceId(1),
                    "Broken",
                    ResourceFamily.Raw,
                    ResourceUsageFlags.Construction,
                    isSettlementStored: true,
                    startingSettlementAmount: -1)
            };

            Assert.Throws<InvalidOperationException>(() => ResourceCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_MissingDisplayName_Throws()
        {
            var definitions = new[]
            {
                new ResourceDefinition(
                    new ResourceId(1),
                    "",
                    ResourceFamily.Raw,
                    ResourceUsageFlags.Construction,
                    isSettlementStored: true,
                    startingSettlementAmount: 0)
            };

            Assert.Throws<InvalidOperationException>(() => ResourceCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_CurrentCatalog_DoesNotThrowAndCoversDesignLockFamilies()
        {
            Assert.DoesNotThrow(() => ResourceCatalogValidator.Validate(ResourceCatalog.All));
            Assert.That(ResourceCatalog.All.Count, Is.EqualTo(9));

            AssertResource(ResourceCatalog.WoodId, ResourceFamily.Raw, ResourceUsageFlags.Construction, 50);
            AssertResource(ResourceCatalog.StoneId, ResourceFamily.Raw, ResourceUsageFlags.Construction, 25);
            AssertResource(ResourceCatalog.PlanksId, ResourceFamily.Refined, ResourceUsageFlags.ProductionOutput, 0);
            AssertResource(ResourceCatalog.SimplePartsId, ResourceFamily.Refined, ResourceUsageFlags.ProductionOutput, 0);
            AssertResource(ResourceCatalog.RepairKitsId, ResourceFamily.Stability, ResourceUsageFlags.Repair, 0);
            AssertResource(ResourceCatalog.FoodId, ResourceFamily.Flow, ResourceUsageFlags.Upkeep, 0);
            AssertResource(ResourceCatalog.FuelId, ResourceFamily.Flow, ResourceUsageFlags.Fuel, 0);
            AssertResource(ResourceCatalog.ResearchDataId, ResourceFamily.Progression, ResourceUsageFlags.Progression, 0);
            AssertResource(ResourceCatalog.MedicineId, ResourceFamily.Stability, ResourceUsageFlags.Upkeep, 0);

            var families = new HashSet<ResourceFamily>();
            for (var i = 0; i < ResourceCatalog.All.Count; i++)
                families.Add(ResourceCatalog.All[i].Family);

            Assert.That(families.SetEquals(new[]
            {
                ResourceFamily.Raw,
                ResourceFamily.Flow,
                ResourceFamily.Refined,
                ResourceFamily.Progression,
                ResourceFamily.Stability
            }), Is.True);
        }

        private static void AssertResource(
            ResourceId resourceId,
            ResourceFamily family,
            ResourceUsageFlags expectedUsage,
            int startingAmount)
        {
            ref readonly var definition = ref ResourceCatalog.Get(resourceId);
            Assert.That(definition.Family, Is.EqualTo(family));
            Assert.That(definition.DisplayName, Is.Not.Empty);
            Assert.That(definition.Usage.HasFlag(expectedUsage), Is.True);
            Assert.That(definition.IsSettlementStored, Is.True);
            Assert.That(definition.StartingSettlementAmount, Is.EqualTo(startingAmount));
        }

        private static ResourceDefinition CreateDefinition(ResourceId id, ResourceFamily family)
        {
            return new ResourceDefinition(
                id,
                $"Resource {id.Value}",
                family,
                ResourceUsageFlags.Construction,
                isSettlementStored: true,
                startingSettlementAmount: 0);
        }
    }
}
