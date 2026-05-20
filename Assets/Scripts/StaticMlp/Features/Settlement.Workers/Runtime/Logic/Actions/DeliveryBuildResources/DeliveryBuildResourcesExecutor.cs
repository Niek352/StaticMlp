using StaticMlp.Features.AiBots;
using StaticMlp.Features.Buildings;
using StaticMlp.Game.Components;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Settlement.Workers
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
            ref readonly var siteResources = ref site.Read<ConstructionResources>();
            if (!SettlementConstructionRules.CanDepositResources(in siteState))
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
                task.ElapsedTicks++;
                return;
            }

            var sharedStorageEntity = SettlementSharedResourcesQuery.GetServerEntity();
            var sharedStorage = sharedStorageEntity.Read<SettlementSharedResources>();
            if (!SettlementConstructionRules.TryPlanResourceDeposit(
                    in siteState,
                    in siteResources,
                    sharedStorage.GetAmount(ResourceCatalog.WoodId),
                    sharedStorage.GetAmount(ResourceCatalog.StoneId),
                    sharedStorage.GetAmount(ResourceCatalog.PlanksId),
                    sharedStorage.GetAmount(ResourceCatalog.SimplePartsId),
                    siteResources.RemainingWood,
                    siteResources.RemainingStone,
                    siteResources.RemainingPlanks,
                    siteResources.RemainingSimpleParts,
                    out var acceptedWood,
                    out var acceptedStone,
                    out var acceptedPlanks,
                    out var acceptedSimpleParts))
            {
                _transitions.SwitchToIdle(entity, ref task);
                return;
            }

            SW.SendEvent(new DepositConstructionResourcesEvent(
                site.GID,
                acceptedWood,
                acceptedStone,
                acceptedPlanks,
                acceptedSimpleParts));
            task.ElapsedTicks++;
        }
    }
}
