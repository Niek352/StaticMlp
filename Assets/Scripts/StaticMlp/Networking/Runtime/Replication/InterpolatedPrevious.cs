using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public struct InterpolatedPrevious<T> : IComponent, IInterpolatedPrevious
        where T : struct, IComponent {
        public T Value;

        public InterpolatedPrevious(T value) {
            Value = value;
        }
    }
}
