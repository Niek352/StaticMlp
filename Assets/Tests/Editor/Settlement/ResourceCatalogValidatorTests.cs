using System;
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
                    ResourceFamily.Raw,
                    ResourceUsageFlags.Construction,
                    isSettlementStored: true,
                    startingSettlementAmount: -1)
            };

            Assert.Throws<InvalidOperationException>(() => ResourceCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_CurrentCatalog_DoesNotThrowAndUsesRawFamily()
        {
            Assert.DoesNotThrow(() => ResourceCatalogValidator.Validate(ResourceCatalog.All));
            Assert.That(ResourceCatalog.Get(ResourceCatalog.WoodId).Family, Is.EqualTo(ResourceFamily.Raw));
            Assert.That(ResourceCatalog.Get(ResourceCatalog.StoneId).Family, Is.EqualTo(ResourceFamily.Raw));
        }

        private static ResourceDefinition CreateDefinition(ResourceId id, ResourceFamily family)
        {
            return new ResourceDefinition(
                id,
                family,
                ResourceUsageFlags.Construction,
                isSettlementStored: true,
                startingSettlementAmount: 0);
        }
    }
}
