using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Combat
{
    public struct AddStatusData : IComponent
    {
        public StatusType Type;
        public float Duration;
        public float TickInterval;
        public float Power;
        public byte Stacks;
    }
}
