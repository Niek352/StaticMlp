using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiActions
{
    public sealed class AttackEnemyActionPackage : IAiActionPackage
    {
        private static readonly AttackEnemyCommandTargetBinder CommandBinder = new();

        public AiTaskType TaskType => AiTaskType.AttackEnemy;
        public IAiActionVariableCollector VariableCollector => null;
        public IAiActionCommandTargetBinder ManualCommandTargetBinder => CommandBinder;
        public IReadOnlyList<AiBlackboardFloatBinding> UtilityBindings => AttackEnemyVariableBindings.Bindings;
        public IReadOnlyList<AiBehaviorTaskContribution> UtilityTaskContributions => AttackEnemyVariableBindings.UtilityTaskContributions;

        public IAiTaskExecutor CreateExecutor(AiTaskExecutionTransitions transitions)
        {
            return new AttackEnemyExecutor(transitions);
        }

        private sealed class AttackEnemyCommandTargetBinder : IAiActionCommandTargetBinder
        {
            public bool TryBind(SW.Entity bot, EntityGID target)
            {
                return EnemyTargetCommandBinding.TryBind(bot, target);
            }
        }
    }
}
