namespace StaticMlp.Networking {
    public enum NetPacketType : byte {
        Hello = 1,
        Welcome = 2,
        Spawn = 10,
        Despawn = 11,
        OwnershipChanged = 12,
        Snapshot = 13,
        ChunkLease = 14,
        EntitySnapshotBatch = 20,
        NetworkEvent = 30,
        NetworkEventBatch = 31,
        Ping = 40,
        Pong = 41
    }
}
