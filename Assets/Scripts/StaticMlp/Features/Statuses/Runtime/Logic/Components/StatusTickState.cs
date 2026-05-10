using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Statuses
{
    public struct StatusTickState : IComponent
    {
        public float Interval;
        public float Timer;
    }
}
