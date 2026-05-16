using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public struct SpawnMessage {
        public EntityGID Gid;
        public byte EntityType;
        public ushort NetworkSchemaVersion;
        public NetworkPeerId Owner;
        public NetworkAuthority Authority;
        public ushort NetworkArchetypeId;
        public byte[] SnapshotPayload;
    }
}
