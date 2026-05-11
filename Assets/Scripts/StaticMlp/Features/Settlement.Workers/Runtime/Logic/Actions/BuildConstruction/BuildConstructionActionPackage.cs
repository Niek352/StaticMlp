using System.Collections.Generic;
using StaticMlp.Features.AiBots;

namespace StaticMlp.Features.Settlement.Workers
{
    public sealed class BuildConstructionActionPackage : IAiActionPackage
    {
        private static readonly BuildConstructionCollectVariables Collector = new();

        public AiTaskType TaskType => AiTaskType.BuildConstruction;
        public IAiActionVariableCollector VariableCollector => Collector;
        public IAiActionCommandTargetBinder ManualCommandTargetBinder => null;
        public IReadOnlyList<AiBlackboardFloatBinding> UtilityBindings => BuildConstructionVariableBindings.Bindings;

        public IAiTaskExecutor CreateExecutor(AiTaskExecutionTransitions transitions)
        {
            return new BuildConstructionExecutor(transitions);
        }
    }
}
