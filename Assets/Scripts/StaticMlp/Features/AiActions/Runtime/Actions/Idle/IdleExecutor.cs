using StaticMlp.Features.AiBots;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.AiActions
{
    public sealed class IdleExecutor : AiTaskExecutorBase
    {
        private readonly AiTaskExecutionTransitions _transitions;

        public IdleExecutor(AiTaskExecutionTransitions transitions)
        {
            _transitions = transitions;
        }

        public override AiTaskType TaskType => AiTaskType.Idle;

        public override void Enter(SW.Entity entity, ref AiTaskState task)
        {
            _transitions.StopMovement(entity);
        }

        public override void Execute(SW.Entity entity, ref AiTaskState task)
        {
            task.Timer += Time.deltaTime;
        }
    }
}
