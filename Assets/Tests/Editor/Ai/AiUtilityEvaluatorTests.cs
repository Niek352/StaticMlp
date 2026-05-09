using NUnit.Framework;
using StaticMlp.Features.AiBots;
using StaticMlp.Game.Components.Buildings;
using UnityEngine;

namespace StaticMlp.Tests.Ai
{
    public sealed class AiUtilityEvaluatorTests
    {
        [Test]
        public void MonsterBehavior_PrefersAttackEnemy_WhenHealthyAndEnemyIsNear()
        {
            using var scope = new AiTestServerWorldScope();
            var bot = scope.CreateBot(Vector3.zero, AiBehaviorIds.Monster);
            var enemy = scope.CreateBot(new Vector3(2f, 0f, 0f));

            AiBlackboardAccess.SetFloat(bot, AiCoreVariableIds.Health01, 1f);
            AiBlackboardAccess.SetFloat(bot, AiCoreVariableIds.Fear, 0.05f);
            AiBlackboardAccess.SetEntity(bot, AiCoreVariableIds.Enemy, enemy.GID);
            AiBlackboardAccess.SetFloat(bot, AiCoreVariableIds.EnemyDistance, 2f);

            Assert.That(scope.Catalog.TryGetBehavior(AiBehaviorIds.Monster, out var behavior), Is.True);
            var selectedTask = AiUtilityEvaluator.SelectBestTask(bot, scope.Catalog, in behavior);
            Assert.That(selectedTask, Is.EqualTo(AiTaskType.AttackEnemy));
        }

        [Test]
        public void MonsterBehavior_PrefersFlee_WhenFearIsHighAndEnemyIsNear()
        {
            using var scope = new AiTestServerWorldScope();
            var bot = scope.CreateBot(Vector3.zero, AiBehaviorIds.Monster);
            var enemy = scope.CreateBot(new Vector3(2f, 0f, 0f));

            AiBlackboardAccess.SetFloat(bot, AiCoreVariableIds.Health01, 0.15f);
            AiBlackboardAccess.SetFloat(bot, AiCoreVariableIds.Fear, 0.95f);
            AiBlackboardAccess.SetEntity(bot, AiCoreVariableIds.Enemy, enemy.GID);
            AiBlackboardAccess.SetFloat(bot, AiCoreVariableIds.EnemyDistance, 2f);

            Assert.That(scope.Catalog.TryGetBehavior(AiBehaviorIds.Monster, out var behavior), Is.True);
            var selectedTask = AiUtilityEvaluator.SelectBestTask(bot, scope.Catalog, in behavior);
            Assert.That(selectedTask, Is.EqualTo(AiTaskType.Flee));
        }

        [Test]
        public void PeacefulBuilderBehavior_PrefersBuildConstruction_WhenBuildableSiteExists()
        {
            using var scope = new AiTestServerWorldScope();
            var bot = scope.CreateBot(Vector3.zero, AiBehaviorIds.PeacefulBuilder);
            scope.CreateConstructionSite(new Vector3(3f, 0f, 0f), ConstructionPhase.ReadyToBuild, resourcesComplete: true);

            AiBlackboardAccess.SetFloat(bot, AiCoreVariableIds.Health01, 1f);
            AiBlackboardAccess.SetFloat(bot, AiCoreVariableIds.Fear, 0.05f);
            AiBlackboardAccess.Remove(bot, AiCoreVariableIds.Leader);
            AiBlackboardAccess.Remove(bot, AiCoreVariableIds.Enemy);

            scope.Catalog.CollectVariables(bot);

            Assert.That(scope.Catalog.TryGetBehavior(AiBehaviorIds.PeacefulBuilder, out var behavior), Is.True);
            var selectedTask = AiUtilityEvaluator.SelectBestTask(bot, scope.Catalog, in behavior);
            Assert.That(selectedTask, Is.EqualTo(AiTaskType.BuildConstruction));
        }

        [Test]
        public void PeacefulBuilderBehavior_DoesNotChooseFlee_WhenFearIsHighAndEnemyIsNear()
        {
            using var scope = new AiTestServerWorldScope();
            var bot = scope.CreateBot(Vector3.zero, AiBehaviorIds.PeacefulBuilder);
            var enemy = scope.CreateBot(new Vector3(2f, 0f, 0f), AiBehaviorIds.Monster);

            AiBlackboardAccess.SetFloat(bot, AiCoreVariableIds.Health01, 0.15f);
            AiBlackboardAccess.SetFloat(bot, AiCoreVariableIds.Fear, 0.95f);
            AiBlackboardAccess.SetEntity(bot, AiCoreVariableIds.Enemy, enemy.GID);
            AiBlackboardAccess.SetFloat(bot, AiCoreVariableIds.EnemyDistance, 2f);
            AiBlackboardAccess.Remove(bot, AiCoreVariableIds.Leader);

            Assert.That(scope.Catalog.TryGetBehavior(AiBehaviorIds.PeacefulBuilder, out var behavior), Is.True);
            var selectedTask = AiUtilityEvaluator.SelectBestTask(bot, scope.Catalog, in behavior);
            Assert.That(selectedTask, Is.Not.EqualTo(AiTaskType.Flee));
        }
    }
}
