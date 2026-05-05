using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    /// <summary>
    /// Local runtime marker for networked entities with replicated data pending collection.
    /// </summary>
    public struct NetworkDirty : ITag { }
}
