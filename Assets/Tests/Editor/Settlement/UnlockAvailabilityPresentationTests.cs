using System;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Tests.Settlement
{
    public sealed class UnlockAvailabilityPresentationTests
    {
        [Test]
        public void AvailableRecipesQuery_FiltersLockedRecipesFromStationChoices()
        {
            using var scope = new ClientUnlockWorldScope(settlementLevel: 1);
            var recipes = new[]
            {
                CreateRecipe(new ProductionRecipeId(101), "available", UnlockRequirement.None),
                CreateRecipe(new ProductionRecipeId(102), "locked", UnlockRequirement.SettlementLevel(2))
            };

            var count = 0;
            ProductionRecipeDefinition available = default;
            foreach (var recipe in AvailableRecipesQuery.Filter(recipes))
            {
                available = recipe;
                count++;
            }

            Assert.That(count, Is.EqualTo(1));
            Assert.That(available.Code, Is.EqualTo("available"));
        }

        private static ProductionRecipeDefinition CreateRecipe(
            ProductionRecipeId id,
            string code,
            UnlockRequirement unlockRequirement)
        {
            return new ProductionRecipeDefinition(
                ProductionStationIds.Workbench,
                id,
                code,
                new[] { new ResourceAmount(ResourceCatalog.WoodId, 1) },
                new[] { new ResourceAmount(ResourceCatalog.PlanksId, 1) },
                1f,
                unlockRequirement: unlockRequirement);
        }

        private sealed class ClientUnlockWorldScope : IDisposable
        {
            public ClientUnlockWorldScope(int settlementLevel)
            {
                if (CW.Status != WorldStatus.NotCreated)
                    CW.Destroy();

                CW.Create(WorldConfig.Default());
                CW.Types().RegisterAll(
                    typeof(ClientCoreWT).Assembly,
                    typeof(SettlementSharedResourcesGameplayFeature).Assembly);
                CW.Initialize();
                CW.SetResource(new SettlementProgressionState { SettlementLevel = settlementLevel });
                CW.SetResource(new ClientSettlementUnlockState());
            }

            public void Dispose()
            {
                if (CW.Status != WorldStatus.NotCreated)
                    CW.Destroy();
            }
        }
    }
}
