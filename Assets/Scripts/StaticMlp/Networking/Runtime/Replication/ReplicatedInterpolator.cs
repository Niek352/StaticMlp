using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public delegate void ReplicatedInterpolator<T>(
        in T previous,
        in T current,
        ref T interpolated,
        float alpha
    ) where T : struct, IComponent;
}
