using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    /// <summary>
    /// Local per-entity replication metadata. Not serialized as gameplay state.
    /// </summary>
    public struct NetworkReplicationState : IComponent {
        public uint DataVersion;
        public uint ShapeVersion;
    }
}
