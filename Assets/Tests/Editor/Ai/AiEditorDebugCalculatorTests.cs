using System.Linq;
using NUnit.Framework;
using StaticMlp.Editor.Ai;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Combat;
using UnityEngine;

namespace StaticMlp.Tests.Ai
{
    public sealed class AiEditorDebugCalculatorTests
    {
        [Test]
        public void Snapshot_MatchesRuntimeUtilityScoresAndFlags()
        {
            using var scope = new AiTestServerWorldScope();
            var bot = scope.CreateBot(Vector3.zero, CombatEnemyBehaviorIds.Monster);
            var enemy = scope.CreateBot(new Vector3(2f, 0f, 0f));

            AiBlackboardAccess.SetFloat(bot, AiCoreVariableIds.Health01, 1f);
            AiBlackboardAccess.SetFloat(bot, AiCoreVariableIds.Fear, 0.05f);
            AiBlackboardAccess.SetEntity(bot, AiCoreVariableIds.Enemy, enemy.GID);
            AiBlackboardAccess.SetFloat(bot, AiCoreVariableIds.EnemyDistance, 2f);

            ref var taskState = ref bot.Mut<AiTaskState>();
            taskState.Task = AiTaskType.AttackEnemy;
            taskState.ActiveTask = AiTaskType.AttackEnemy;
            taskState.HasActiveTask = true;

            Assert.That(scope.Catalog.TryGetBehavior(CombatEnemyBehaviorIds.Monster, out var behavior), Is.True);
            var brain = bot.Read<AiBrain>();
            var snapshot = AiEditorDebugCalculator.BuildSnapshot(bot, scope.Catalog, behavior, brain, taskState);

            var attackDefinition = behavior.Tasks.First(task => task.Task == AiTaskType.AttackEnemy);
            var attackRow = snapshot.Tasks.First(task => task.TaskType == AiTaskType.AttackEnemy);

            Assert.That(
                attackRow.Score,
                Is.EqualTo(AiUtilityEvaluator.EvaluateTask(bot, scope.Catalog, attackDefinition.Considerations)).Within(0.0001f));
            Assert.That(attackRow.IsSelectedTask, Is.True);
            Assert.That(attackRow.IsActiveTask, Is.True);
            Assert.That(attackRow.IsBestTask, Is.True);
            Assert.That(attackRow.Considerations.Length, Is.EqualTo(4));
            Assert.That(attackRow.Considerations[0].VariableName, Is.EqualTo("health01"));
        }

        [Test]
        public void Snapshot_ProducesZeroScoreForTaskWithoutConsiderations()
        {
            using var scope = new AiTestServerWorldScope();
            var bot = scope.CreateBot(Vector3.zero);
            var behavior = new AiBehaviorDefinition
            {
                BehaviorId = 999,
                Tasks = new[]
                {
                    new UtilityTaskDefinition
                    {
                        Task = AiTaskType.Idle,
                        Considerations = null
                    }
                }
            };

            var snapshot = AiEditorDebugCalculator.BuildSnapshot(
                bot,
                scope.Catalog,
                behavior,
                bot.Read<AiBrain>(),
                bot.Read<AiTaskState>());

            Assert.That(snapshot.Tasks, Has.Length.EqualTo(1));
            Assert.That(snapshot.Tasks[0].Score, Is.EqualTo(0f));
            Assert.That(snapshot.Tasks[0].Considerations, Is.Empty);
            Assert.That(snapshot.BestTaskIndex, Is.EqualTo(-1));
        }
    }
}
