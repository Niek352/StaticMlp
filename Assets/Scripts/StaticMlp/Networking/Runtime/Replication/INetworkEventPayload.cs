namespace StaticMlp.Networking.Replication {
    internal interface INetworkEventPayload {
        void Write(ref NetworkWriter writer);
        bool TryApplyToClient(NetworkPeerId sourcePeer, int receiveOrder);
        bool TryApplyToServer(NetworkPeerId sourcePeer, int receiveOrder);
    }
}
