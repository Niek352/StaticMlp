using StaticMlp.Features.AiBots;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.AiActions
{
    public sealed class FleeExecutor : AiTaskExecutorBase
    {
        private readonly AiTaskExecutionTransitions _transitions;

        public FleeExecutor(AiTaskExecutionTransitions transitions)
        {
            _transitions = transitions;
        }

        public override AiTaskType TaskType => AiTaskType.Flee;

        public override void Execute(SW.Entity entity, ref AiTaskState task)
        {
            ref readonly var state = ref entity.Read<CharacterNetState>();
            var lastKnownEnemyPosition = AiBlackboardAccess.TryGetVector(
                entity,
                AiCoreVariableIds.LastKnownEnemyPosition,
                out var storedPosition)
                ? storedPosition
                : state.Position;

            var away = state.Position - lastKnownEnemyPosition;
            if (away.sqrMagnitude < 0.001f)
                away = Vector3.forward;

            away.Normalize();
            entity.Set(new AiMoveRequest
            {
                Destination = state.Position + away * 8f,
                StopDistance = 0.5f
            });
            task.ElapsedTicks++;
        }
    }
}
