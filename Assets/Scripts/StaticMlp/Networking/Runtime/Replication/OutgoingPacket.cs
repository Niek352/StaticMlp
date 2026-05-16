namespace StaticMlp.Networking.Replication {
    public struct OutgoingPacket {
        public NetworkPeerId Peer;
        public NetDelivery Delivery;
        public byte[] Payload;
    }
}
