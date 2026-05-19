using System;
using NUnit.Framework;
using StaticMlp.Features.CombatDirector;

namespace StaticMlp.Tests.CombatDirector
{
    public sealed class EnemySpawnCatalogTests
    {
        [Test]
        public void DefaultCatalog_ValidatesAndExposesExpectedRoles()
        {
            var catalog = EnemySpawnCatalog.CreateDefault();

            Assert.That(catalog.All.Count, Is.EqualTo(3));
            Assert.That(catalog.Get(EnemyRole.Swarmer).MaxCountPerWave, Is.EqualTo(18));
            Assert.That(catalog.Get(EnemyRole.Marker).BudgetCost, Is.EqualTo(4f));
            Assert.That(catalog.Get(EnemyRole.AnchorElite).BudgetCost, Is.EqualTo(10f));
        }

        [Test]
        public void DuplicateRole_Throws()
        {
            var definitions = new[]
            {
                new EnemySpawnDefinition(EnemyRole.Swarmer, budgetCost: 1f, minCountPerWave: 6, maxCountPerWave: 18),
                new EnemySpawnDefinition(EnemyRole.Swarmer, budgetCost: 2f, minCountPerWave: 1, maxCountPerWave: 2)
            };

            var exception = Assert.Throws<InvalidOperationException>(() => new EnemySpawnCatalog(definitions));

            Assert.That(exception!.Message, Does.Contain("Duplicate enemy spawn definition"));
        }

        [Test]
        public void ZeroBudgetCost_Throws()
        {
            var definitions = new[]
            {
                new EnemySpawnDefinition(EnemyRole.Marker, budgetCost: 0f, minCountPerWave: 1, maxCountPerWave: 1)
            };

            var exception = Assert.Throws<InvalidOperationException>(() => new EnemySpawnCatalog(definitions));

            Assert.That(exception!.Message, Does.Contain("non-positive budget cost"));
        }

        [Test]
        public void InvertedMinMaxCount_Throws()
        {
            var definitions = new[]
            {
                new EnemySpawnDefinition(EnemyRole.AnchorElite, budgetCost: 10f, minCountPerWave: 2, maxCountPerWave: 1)
            };

            var exception = Assert.Throws<InvalidOperationException>(() => new EnemySpawnCatalog(definitions));

            Assert.That(exception!.Message, Does.Contain("inverts min/max wave counts"));
        }
    }
}
