namespace StaticMlp.Networking.Replication {
    internal interface INetworkEventPayload {
        void Write(ref NetworkWriter writer);
        void ApplyToClient(NetworkPeerId sourcePeer);
        void ApplyToServer(NetworkPeerId sourcePeer);
    }
}
