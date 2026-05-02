namespace StaticMlp.Networking {
    public enum NetPacketType : byte {
        Hello = 1,
        Welcome = 2,
        Spawn = 10,
        Despawn = 11,
        OwnershipChanged = 12,
        ComponentBatch = 20,
        NetworkEvent = 30,
        Ping = 40,
        Pong = 41
    }
}
