using System;
using NUnit.Framework;
using StaticMlp.Features.Npc;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Tests.Npc
{
    public sealed class NpcIncubationRecipeCatalogTests
    {
        [Test]
        public void CurrentCatalog_Validates()
        {
            Assert.DoesNotThrow(() => NpcIncubationRecipeCatalogValidator.Validate(NpcIncubationRecipeCatalog.All));
        }

        [Test]
        public void Get_ExistingRecipe_ReturnsRecipe()
        {
            var recipe = NpcIncubationRecipeCatalog.Get(NpcIncubationRecipeCatalog.ResearcherRecipeId);

            Assert.That(recipe.Id, Is.EqualTo(NpcIncubationRecipeCatalog.ResearcherRecipeId));
            Assert.That(recipe.ResultNpc, Is.EqualTo(NpcDefinitionCatalog.IncubatedResearcherId));
        }

        [Test]
        public void Validate_DuplicateId_Throws()
        {
            var definitions = new[]
            {
                CreateRecipe(new NpcIncubationRecipeId(1), NpcDefinitionCatalog.IncubatedResearcherId),
                CreateRecipe(new NpcIncubationRecipeId(1), NpcDefinitionCatalog.IncubatedResearcherId)
            };

            Assert.Throws<InvalidOperationException>(() => NpcIncubationRecipeCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_InvalidResultNpc_Throws()
        {
            var definitions = new[]
            {
                CreateRecipe(
                    new NpcIncubationRecipeId(1),
                    NpcDefinitionCatalog.RescuedSpecialistId)
            };

            Assert.Throws<InvalidOperationException>(() => NpcIncubationRecipeCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_InvalidResourceId_Throws()
        {
            var definitions = new[]
            {
                new NpcIncubationRecipeDefinition(
                    new NpcIncubationRecipeId(1),
                    NpcDefinitionCatalog.IncubatedResearcherId,
                    new[] { new ResourceAmount(new ResourceId(999), 10) },
                    requiredStationTier: 1,
                    durationSeconds: 60f)
            };

            Assert.Throws<InvalidOperationException>(() => NpcIncubationRecipeCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_ZeroDuration_Throws()
        {
            var definitions = new[]
            {
                CreateRecipe(
                    new NpcIncubationRecipeId(1),
                    NpcDefinitionCatalog.IncubatedResearcherId,
                    durationSeconds: 0f)
            };

            Assert.Throws<InvalidOperationException>(() => NpcIncubationRecipeCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_ZeroStationTier_Throws()
        {
            var definitions = new[]
            {
                new NpcIncubationRecipeDefinition(
                    new NpcIncubationRecipeId(1),
                    NpcDefinitionCatalog.IncubatedResearcherId,
                    new[] { new ResourceAmount(ResourceCatalog.WoodId, 10) },
                    requiredStationTier: 0,
                    durationSeconds: 60f)
            };

            Assert.Throws<InvalidOperationException>(() => NpcIncubationRecipeCatalogValidator.Validate(definitions));
        }

        [Test]
        public void Validate_EmptyCosts_Throws()
        {
            var definitions = new[]
            {
                new NpcIncubationRecipeDefinition(
                    new NpcIncubationRecipeId(1),
                    NpcDefinitionCatalog.IncubatedResearcherId,
                    Array.Empty<ResourceAmount>(),
                    requiredStationTier: 1,
                    durationSeconds: 60f)
            };

            Assert.Throws<InvalidOperationException>(() => NpcIncubationRecipeCatalogValidator.Validate(definitions));
        }

        private static NpcIncubationRecipeDefinition CreateRecipe(
            NpcIncubationRecipeId id,
            NpcDefinitionId resultNpc,
            float durationSeconds = 60f)
        {
            return new NpcIncubationRecipeDefinition(
                id,
                resultNpc,
                new[] { new ResourceAmount(ResourceCatalog.WoodId, 10) },
                requiredStationTier: 1,
                durationSeconds);
        }
    }
}
