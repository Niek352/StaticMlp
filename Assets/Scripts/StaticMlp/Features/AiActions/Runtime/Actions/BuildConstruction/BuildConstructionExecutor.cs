using StaticMlp.Features.AiBots;
using StaticMlp.Features.Buildings;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Game.Components.Buildings;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.AiActions
{
    public sealed class BuildConstructionExecutor : AiTaskExecutorBase
    {
        private const float BuildInteractionRange = 4f;
        private const float BuildWorkPerSecond = 8f;

        private readonly AiTaskExecutionTransitions _transitions;

        public BuildConstructionExecutor(AiTaskExecutionTransitions transitions)
        {
            _transitions = transitions;
        }

        public override AiTaskType TaskType => AiTaskType.BuildConstruction;

        public override void Execute(SW.Entity entity, ref AiTaskState task)
        {
            var fixedStepSeconds = SW.GetResource<SimulationTime>().FixedStepSeconds;
            if (!AiBlackboardAccess.TryGetEntity(entity, BuildConstructionCollectVariables.BuildTargetSite, out var target)
                || !ConstructionSiteQuery.TryGetBuildableSite(target, out var site))
            {
                _transitions.SwitchToIdle(entity, ref task);
                return;
            }

            ref readonly var siteState = ref site.Read<ConstructionSiteState>();
            ref readonly var siteResources = ref site.Read<ConstructionResources>();
            if (!ConstructionRules.CanBuild(in siteState, in siteResources))
            {
                _transitions.SwitchToIdle(entity, ref task);
                return;
            }

            ref readonly var siteTransform = ref site.Read<ConstructionTransform>();
            ref readonly var characterState = ref entity.Read<CharacterNetState>();
            var toSite = siteTransform.Position - characterState.Position;
            if (toSite.sqrMagnitude > BuildInteractionRange * BuildInteractionRange)
            {
                entity.Set(new AiMoveRequest
                {
                    Destination = siteTransform.Position,
                    StopDistance = BuildInteractionRange - 0.5f
                });
                task.ElapsedTicks++;
                return;
            }

            _transitions.StopMovement(entity);

            ref var mutableSiteState = ref ReplicationMut.Mut<ConstructionSiteState>(site);
            ref var mutableProgress = ref ReplicationMut.Mut<ConstructionProgress>(site);
            if (!ConstructionRules.ApplyBuildWork(
                    ref mutableSiteState,
                    ref mutableProgress,
                    in siteResources,
                    BuildWorkPerSecond * fixedStepSeconds,
                    BuildWorkPerSecond))
            {
                _transitions.SwitchToIdle(entity, ref task);
                return;
            }

            task.ElapsedTicks++;
            if (mutableProgress.IsComplete)
                _transitions.SwitchToIdle(entity, ref task);
        }
    }
}
