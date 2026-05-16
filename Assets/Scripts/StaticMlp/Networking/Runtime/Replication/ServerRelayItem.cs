namespace StaticMlp.Networking.Replication {
    public readonly struct ServerRelayItem {
        public readonly NetworkPeerId SourcePeer;
        public readonly byte[] Payload;
        public readonly NetDelivery Delivery;

        public ServerRelayItem(NetworkPeerId sourcePeer, byte[] payload, NetDelivery delivery) {
            SourcePeer = sourcePeer;
            Payload = payload;
            Delivery = delivery;
        }
    }
}
