using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.AiBots
{
    public struct AiTaskState : IComponent
    {
        public AiTaskType Task;
        public AiTaskType ActiveTask;
        public bool HasActiveTask;
        public byte Step;
        public uint ElapsedTicks;
    }
}
