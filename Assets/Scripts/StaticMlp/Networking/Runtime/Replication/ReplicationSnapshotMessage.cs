namespace StaticMlp.Networking.Replication {
    public struct ReplicationSnapshotMessage {
        public ReplicationSnapshotKind Kind;
        public ushort ClusterId;
        public uint ChunkIdx;
        public bool Gzip;
        public byte[] Payload;
    }
}
