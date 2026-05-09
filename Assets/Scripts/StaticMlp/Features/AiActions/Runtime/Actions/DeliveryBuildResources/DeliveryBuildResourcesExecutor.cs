using StaticMlp.Features.AiBots;
using StaticMlp.Features.Buildings;
using StaticMlp.Game.Components;
using StaticMlp.Game.Components.Buildings;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.AiActions
{
    public sealed class DeliveryBuildResourcesExecutor : AiTaskExecutorBase
    {
        private const float InteractionRange = 4f;

        private readonly AiTaskExecutionTransitions _transitions;

        public DeliveryBuildResourcesExecutor(AiTaskExecutionTransitions transitions)
        {
            _transitions = transitions;
        }

        public override AiTaskType TaskType => AiTaskType.DeliveryResourceToBuilding;

        public override void Execute(SW.Entity entity, ref AiTaskState task)
        {
            if (!AiBlackboardAccess.TryGetEntity(entity, DeliveryBuildResourcesCollectVariables.TargetSite, out var target)
                || !ConstructionSiteQuery.TryGetConstructionSite(target, out var site))
            {
                _transitions.SwitchToIdle(entity, ref task);
                return;
            }

            ref readonly var siteState = ref site.Read<ConstructionSiteState>();
            if (!ConstructionRules.CanDepositResources(in siteState))
            {
                _transitions.SwitchToIdle(entity, ref task);
                return;
            }

            ref readonly var siteTransform = ref site.Read<ConstructionTransform>();
            ref readonly var characterState = ref entity.Read<CharacterNetState>();
            var toSite = siteTransform.Position - characterState.Position;
            if (toSite.sqrMagnitude > InteractionRange * InteractionRange)
            {
                entity.Set(new AiMoveRequest
                {
                    Destination = siteTransform.Position,
                    StopDistance = InteractionRange - 0.5f
                });
                task.Timer += Time.deltaTime;
                return;
            }
            
            ref var mutableSiteState = ref ReplicationMut.Mut<ConstructionSiteState>(site);
            ref var resources = ref ReplicationMut.Mut<ConstructionResources>(site);

            if (!ConstructionRules.ApplyResourceDeposit(ref mutableSiteState, ref resources, 100, 100))
            {
                _transitions.SwitchToIdle(entity, ref task);
                return;
            }

            task.Timer += Time.deltaTime;
            if (resources.IsComplete)
                _transitions.SwitchToIdle(entity, ref task);
        }
    }
}
