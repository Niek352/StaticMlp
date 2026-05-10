using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.AiBots
{
    public struct AiBrain : IComponent
    {
        public ushort BehaviorId;
        public AiTaskType CurrentTask;
        public uint NextDecisionTick;
    }
}
