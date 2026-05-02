using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Networking.Replication {
    public sealed class ReplicatedInterpolationSystem<T> : ISystem
        where T : struct, IComponent {
        private readonly ReplicatedInterpolator<T> _interpolator;

        public ReplicatedInterpolationSystem(ReplicatedInterpolator<T> interpolator) {
            _interpolator = interpolator;
        }

        public void Update() {
            foreach (var e in CW.Query<All<RemoteOwned, T, InterpolatedPrevious<T>, Interpolated<T>, InterpolatedClock<T>>>().Entities()) {
                ref readonly var current = ref e.Read<T>();
                ref readonly var previous = ref e.Read<InterpolatedPrevious<T>>();
                ref readonly var clock = ref e.Read<InterpolatedClock<T>>();
                ref var interpolated = ref e.Mut<Interpolated<T>>();

                var alpha = clock.Duration <= 0f
                    ? 0.999f
                    : Mathf.Clamp01((Time.time - clock.StartedAt) / clock.Duration);

                if (alpha >= 1f)
                    alpha = 0.999f;

                _interpolator(previous.Value, current, ref interpolated.Value, alpha);
            }
        }
    }
}
