using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public struct Interpolated<T> : IComponent, IInterpolated
        where T : struct, IComponent {
        public T Value;

        public Interpolated(T value) {
            Value = value;
        }
    }
}
