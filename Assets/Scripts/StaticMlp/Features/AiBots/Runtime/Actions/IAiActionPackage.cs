using System.Collections.Generic;

namespace StaticMlp.Features.AiBots
{
    public interface IAiActionPackage
    {
        AiTaskType TaskType { get; }
        IAiActionVariableCollector VariableCollector { get; }
        IAiActionCommandTargetBinder ManualCommandTargetBinder { get; }
        IReadOnlyList<AiBlackboardFloatBinding> UtilityBindings { get; }

        IAiTaskExecutor CreateExecutor(AiTaskExecutionTransitions transitions);
    }
}
