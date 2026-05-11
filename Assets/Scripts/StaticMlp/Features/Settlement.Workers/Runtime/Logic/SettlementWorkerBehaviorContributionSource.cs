using StaticMlp.Features.AiActions;
using StaticMlp.Features.AiBots;

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
        }
    }
}
