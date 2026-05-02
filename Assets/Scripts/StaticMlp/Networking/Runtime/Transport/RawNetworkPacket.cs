namespace StaticMlp.Networking.Transport {
    public readonly struct RawNetworkPacket {
        public readonly NetworkPeerId SourcePeer;
        public readonly byte[] Payload;

        public RawNetworkPacket(NetworkPeerId sourcePeer, byte[] payload) {
            SourcePeer = sourcePeer;
            Payload = payload;
        }
    }
}
