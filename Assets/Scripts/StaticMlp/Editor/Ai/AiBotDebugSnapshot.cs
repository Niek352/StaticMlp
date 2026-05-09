using StaticMlp.Features.AiBots;
using UnityEngine;

namespace StaticMlp.Editor.Ai
{
    public sealed class AiBotDebugSnapshot
    {
        public ushort BehaviorId { get; set; }
        public Vector3 Position { get; set; }
        public AiTaskType CurrentTask { get; set; }
        public AiTaskType SelectedTask { get; set; }
        public AiTaskType ActiveTask { get; set; }
        public bool HasActiveTask { get; set; }
        public AiTaskDebugRow[] Tasks { get; set; }
        public int BestTaskIndex { get; set; }
    }
}
