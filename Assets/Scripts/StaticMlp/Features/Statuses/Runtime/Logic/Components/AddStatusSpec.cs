using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Statuses
{
    public struct AddStatusSpec : IComponent
    {
        public float Duration;
        public float TickInterval;
        public float Power;
        public byte Stacks;
    }
}
