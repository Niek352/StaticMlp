using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Statuses
{
    public struct StatusTickState : IComponent
    {
        public uint IntervalTicks;
        public uint NextTick;
    }
}
