using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public readonly struct DespawnMessage {
        public readonly EntityGID Gid;

        public DespawnMessage(EntityGID gid) {
            Gid = gid;
        }
    }
}
