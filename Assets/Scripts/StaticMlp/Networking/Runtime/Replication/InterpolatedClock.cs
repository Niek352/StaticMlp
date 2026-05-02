using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public struct InterpolatedClock<T> : IComponent, IInterpolatedClock
        where T : struct, IComponent {
        public float StartedAt;
        public float Duration;
    }
}
