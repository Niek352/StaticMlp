namespace StaticMlp.Networking.Replication {
    internal interface INetworkEventPayload {
        void Write(ref NetworkWriter writer);
        bool TryApplyToClient(NetworkPeerId sourcePeer);
        bool TryApplyToServer(NetworkPeerId sourcePeer);
    }
}
