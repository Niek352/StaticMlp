using StaticMlp.Features.AiBots;

namespace StaticMlp.Editor.Ai
{
    public sealed class AiTaskDebugRow
    {
        public AiTaskType TaskType { get; set; }
        public float Score { get; set; }
        public bool IsSelectedTask { get; set; }
        public bool IsActiveTask { get; set; }
        public bool IsBestTask { get; set; }
        public AiConsiderationDebugRow[] Considerations { get; set; }
    }
}
