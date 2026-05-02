namespace StaticMlp.Networking.Replication {
    public sealed class ReplicationSnapshotMessage {
        public ReplicationSnapshotKind Kind;
        public ushort ClusterId;
        public uint ChunkIdx;
        public bool Gzip;
        public byte[] Payload;
    }
}
