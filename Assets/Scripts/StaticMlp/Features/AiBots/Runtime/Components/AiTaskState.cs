using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.AiBots
{
    public struct AiTaskState : IComponent
    {
        public AiTaskType Task;
        public byte Step;
        public float Timer;
    }
}
