namespace StaticMlp.Networking.Replication {
    public sealed class OutgoingPacket {
        public NetworkPeerId Peer;
        public NetDelivery Delivery;
        public byte[] Payload;
    }
}
