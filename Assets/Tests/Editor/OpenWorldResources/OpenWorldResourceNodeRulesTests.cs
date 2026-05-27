using NUnit.Framework;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Features.OpenWorldResources;

namespace StaticMlp.Tests.OpenWorldResources
{
    public sealed class OpenWorldResourceNodeRulesTests
    {
        [Test]
        public void Profiles_ExposePhase2HarvestMetadata()
        {
            AssertProfile(1, 5, 1, 10, OpenWorldResourceHarvestTag.Axe);
            AssertProfile(2, 8, 4, 35, OpenWorldResourceHarvestTag.Blunt | OpenWorldResourceHarvestTag.Lightning);
            AssertProfile(3, 4, 0, 5, OpenWorldResourceHarvestTag.Fire);
            AssertProfile(4, 6, 2, 20, OpenWorldResourceHarvestTag.Rune | OpenWorldResourceHarvestTag.Projectile);
        }

        private static void AssertProfile(
            ushort kindId,
            int startingAmount,
            int armor,
            int resistancePercent,
            OpenWorldResourceHarvestTag preferredTags)
        {
            var placementKind = new ResourcePlacementKindId(kindId);

            Assert.That(OpenWorldResourceNodeRules.StartingAmount(placementKind), Is.EqualTo(startingAmount));
            Assert.That(OpenWorldResourceNodeRules.Armor(placementKind), Is.EqualTo(armor));
            Assert.That(OpenWorldResourceNodeRules.ResistancePercent(placementKind), Is.EqualTo(resistancePercent));
            Assert.That(OpenWorldResourceNodeRules.PreferredHarvestTags(placementKind), Is.EqualTo(preferredTags));
        }
    }
}
