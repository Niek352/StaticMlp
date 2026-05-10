using NUnit.Framework;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Combat;
using StaticMlp.Features.Shared;
using UnityEngine;

namespace StaticMlp.Tests.Ai
{
    public sealed class AiActionCatalogTests
    {
        [Test]
        public void CatalogDiscoversExecutorsAndManualCommandBinders()
        {
            using var scope = new AiTestServerWorldScope();
            var bot = scope.CreateBot(Vector3.zero);
            var leader = scope.CreateBot(new Vector3(3f, 0f, 0f));
            var enemy = scope.CreateBot(new Vector3(5f, 0f, 0f));

            Assert.That(scope.Catalog.ResolveExecutor(AiTaskType.Idle).TaskType, Is.EqualTo(AiTaskType.Idle));
            Assert.That(scope.Catalog.ResolveExecutor(AiTaskType.DeliveryResourceToBuilding).TaskType, Is.EqualTo(AiTaskType.DeliveryResourceToBuilding));
            Assert.That(scope.Catalog.SupportsManualCommand(AiTaskType.FollowLeader), Is.True);
            Assert.That(scope.Catalog.SupportsManualCommand(AiTaskType.BuildConstruction), Is.False);

            Assert.That(scope.Catalog.TryBindManualCommand(AiTaskType.FollowLeader, bot, leader.GID), Is.True);
            Assert.That(AiBlackboardAccess.TryGetEntity(bot, AiCoreVariableIds.Leader, out var leaderValue), Is.True);
            Assert.That(leaderValue, Is.EqualTo(leader.GID));

            Assert.That(scope.Catalog.TryBindManualCommand(AiTaskType.AttackEnemy, bot, enemy.GID), Is.True);
            Assert.That(AiBlackboardAccess.TryGetEntity(bot, AiCoreVariableIds.Enemy, out var enemyValue), Is.True);
            Assert.That(enemyValue, Is.EqualTo(enemy.GID));
        }

        [Test]
        public void CreateBot_AddsHealthComponent()
        {
            using var scope = new AiTestServerWorldScope();
            var bot = scope.CreateBot(Vector3.zero);

            Assert.That(bot.Has<Health>(), Is.True);
            Assert.That(bot.Read<Health>().Current, Is.EqualTo(100f));
            Assert.That(bot.Read<Health>().Max, Is.EqualTo(100f));
        }
    }
}
