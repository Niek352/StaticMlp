using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiActions
{
    public sealed class FleeActionPackage : IAiActionPackage
    {
        private static readonly FleeCommandTargetBinder CommandBinder = new();

        public AiTaskType TaskType => AiTaskType.Flee;
        public IAiActionVariableCollector VariableCollector => null;
        public IAiActionCommandTargetBinder ManualCommandTargetBinder => CommandBinder;
        public IReadOnlyList<AiBlackboardFloatBinding> UtilityBindings => FleeVariableBindings.Bindings;

        public IAiTaskExecutor CreateExecutor(AiTaskExecutionTransitions transitions)
        {
            return new FleeExecutor(transitions);
        }

        private sealed class FleeCommandTargetBinder : IAiActionCommandTargetBinder
        {
            public bool TryBind(SW.Entity bot, EntityGID target)
            {
                return EnemyTargetCommandBinding.TryBind(bot, target);
            }
        }
    }
}
