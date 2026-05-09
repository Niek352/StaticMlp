using StaticMlp.Networking;

namespace StaticMlp.Features.AiBots
{
    public interface IAiTaskExecutor
    {
        AiTaskType TaskType { get; }

        void Enter(SW.Entity entity, ref AiTaskState task);
        void Execute(SW.Entity entity, ref AiTaskState task);
        void Exit(SW.Entity entity, ref AiTaskState task);
    }
}
