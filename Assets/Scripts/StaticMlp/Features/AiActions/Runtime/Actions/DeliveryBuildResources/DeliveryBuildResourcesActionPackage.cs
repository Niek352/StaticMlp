using System.Collections.Generic;
using StaticMlp.Features.AiBots;

namespace StaticMlp.Features.AiActions
{
    public sealed class DeliveryBuildResourcesActionPackage : IAiActionPackage
    {
        private static readonly DeliveryBuildResourcesCollectVariables Collector = new();

        public AiTaskType TaskType => AiTaskType.DeliveryResourceToBuilding;
        public IAiActionVariableCollector VariableCollector => Collector;
        public IAiActionCommandTargetBinder ManualCommandTargetBinder => null;
        public IReadOnlyList<AiBlackboardFloatBinding> UtilityBindings => DeliveryBuildResourcesVariableBindings.Bindings;

        public IAiTaskExecutor CreateExecutor(AiTaskExecutionTransitions transitions)
        {
            return new DeliveryBuildResourcesExecutor(transitions);
        }
    }
}
