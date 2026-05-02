namespace StaticMlp.Networking.Replication {
    public sealed class NetworkEventMessage {
        public NetworkPeerId SourcePeer;
        public ushort EventTypeId;
        public byte[] Payload;
    }
}
