using StaticMlp.Features.AiBots;
using StaticMlp.Features.AiActions;

namespace StaticMlp.Features.Settlement.Workers
{
    public sealed class SettlementWorkerBehaviorContributionSource : IAiBehaviorContributionSource
    {
        public void Register(AiBehaviorTaskRegistry registry)
        {
            registry.Add(
                SettlementWorkerBehaviorIds.PEACEFUL_BUILDER,
                AiTaskType.BuildConstruction,
                BuildConstructionVariableBindings.Considerations);
            registry.Add(
                SettlementWorkerBehaviorIds.PEACEFUL_BUILDER,
                AiTaskType.DeliveryResourceToBuilding,
                DeliveryBuildResourcesVariableBindings.Considerations);
            registry.Add(
                SettlementWorkerBehaviorIds.PEACEFUL_BUILDER,
                AiTaskType.FollowLeader,
                FollowLeaderVariableBindings.Considerations);
            registry.Add(
                SettlementWorkerBehaviorIds.PEACEFUL_GATHERER,
                AiTaskType.Idle,
                considerations: null);
            registry.Add(
                SettlementWorkerBehaviorIds.PEACEFUL_HAULER,
                AiTaskType.Idle,
                considerations: null);
            registry.Add(
                SettlementWorkerBehaviorIds.PEACEFUL_PROCESSOR,
                AiTaskType.Idle,
                considerations: null);
            registry.Add(
                SettlementWorkerBehaviorIds.SETTLEMENT_GUARD,
                AiTaskType.Idle,
                considerations: null);
        }
    }
}
