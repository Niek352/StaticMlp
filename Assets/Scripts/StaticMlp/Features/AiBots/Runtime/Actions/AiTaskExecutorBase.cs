using StaticMlp.Networking;

namespace StaticMlp.Features.AiBots
{
    public abstract class AiTaskExecutorBase : IAiTaskExecutor
    {
        public abstract AiTaskType TaskType { get; }

        public virtual void Enter(SW.Entity entity, ref AiTaskState task)
        {
        }

        public abstract void Execute(SW.Entity entity, ref AiTaskState task);

        public virtual void Exit(SW.Entity entity, ref AiTaskState task)
        {
        }
    }
}
