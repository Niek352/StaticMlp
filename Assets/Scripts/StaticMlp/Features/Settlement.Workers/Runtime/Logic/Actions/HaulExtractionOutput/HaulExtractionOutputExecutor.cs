using StaticMlp.Features.AiBots;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement.Workers
{
    public sealed class HaulExtractionOutputExecutor : AiTaskExecutorBase
    {
        private const float InteractionRange = 4f;

        private readonly AiTaskExecutionTransitions _transitions;

        public HaulExtractionOutputExecutor(AiTaskExecutionTransitions transitions)
        {
            _transitions = transitions;
        }

        public override AiTaskType TaskType => AiTaskType.HaulResources;

        public override void Execute(SW.Entity entity, ref AiTaskState task)
        {
            if (!AiBlackboardAccess.TryGetEntity(entity, HaulExtractionOutputCollectVariables.TargetExtractionBuilding, out var target)
                || !target.TryUnpack<ServerWT>(out var building)
                || !building.Has<ExtractionOperationState>())
            {
                _transitions.SwitchToIdle(entity, ref task);
                return;
            }

            ref readonly var operation = ref building.Read<ExtractionOperationState>();
            if (!ExtractionRules.HasOutput(in operation))
            {
                _transitions.SwitchToIdle(entity, ref task);
                return;
            }

            ref readonly var buildingTransform = ref building.Read<ConstructionTransform>();
            ref readonly var characterState = ref entity.Read<CharacterNetState>();
            var toBuilding = buildingTransform.Position - characterState.Position;
            if (toBuilding.sqrMagnitude > InteractionRange * InteractionRange)
            {
                entity.Set(new AiMoveRequest
                {
                    Destination = buildingTransform.Position,
                    StopDistance = InteractionRange - 0.5f
                });
                task.ElapsedTicks++;
                return;
            }

            _transitions.StopMovement(entity);
            SW.SendEvent(new TransferExtractionOutputToStockpileEvent(
                building.GID,
                operation.OutputResource,
                operation.OutputBufferAmount));
            task.ElapsedTicks++;
        }
    }
}
