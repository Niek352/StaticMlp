using System.Collections.Generic;
using StaticMlp.Features.AiBots;

namespace StaticMlp.Features.Settlement.Workers
{
    public sealed class HaulExtractionOutputActionPackage : IAiActionPackage
    {
        private static readonly HaulExtractionOutputCollectVariables Collector = new();

        public AiTaskType TaskType => AiTaskType.HaulResources;
        public IAiActionVariableCollector VariableCollector => Collector;
        public IAiActionCommandTargetBinder ManualCommandTargetBinder => null;
        public IReadOnlyList<AiBlackboardFloatBinding> UtilityBindings => HaulExtractionOutputVariableBindings.Bindings;

        public IAiTaskExecutor CreateExecutor(AiTaskExecutionTransitions transitions)
        {
            return new HaulExtractionOutputExecutor(transitions);
        }
    }
}
