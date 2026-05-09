using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Networking;

namespace StaticMlp.Features.AiActions
{
    public sealed class IdleActionPackage : IAiActionPackage
    {
        private static readonly IdleCollectVariables Collector = new();
        private static readonly IdleCommandTargetBinder CommandBinder = new();

        public AiTaskType TaskType => AiTaskType.Idle;
        public IAiActionVariableCollector VariableCollector => Collector;
        public IAiActionCommandTargetBinder ManualCommandTargetBinder => CommandBinder;
        public IReadOnlyList<AiBlackboardFloatBinding> UtilityBindings => IdleVariableBindings.Bindings;
        public IReadOnlyList<AiBehaviorTaskContribution> UtilityTaskContributions => IdleVariableBindings.UtilityTaskContributions;

        public IAiTaskExecutor CreateExecutor(AiTaskExecutionTransitions transitions)
        {
            return new IdleExecutor(transitions);
        }

        private sealed class IdleCommandTargetBinder : IAiActionCommandTargetBinder
        {
            public bool TryBind(SW.Entity bot, EntityGID target)
            {
                return true;
            }
        }
    }
}
