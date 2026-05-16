using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    internal sealed class NetworkEventPayload<TEvent> : INetworkEventPayload
        where TEvent : struct, IEvent {
        private readonly NetworkEventRegistry.Writer<TEvent> _writer;
        private readonly TEvent _evt;

        public NetworkEventPayload(NetworkEventRegistry.Writer<TEvent> writer, in TEvent evt) {
            _writer = writer;
            _evt = evt;
        }

        public void Write(ref NetworkWriter writer) {
            _writer(ref writer, in _evt);
        }

        public bool TryApplyToClient(NetworkPeerId sourcePeer, int receiveOrder) {
            return CW.SendEvent(new NetworkEventFromServer<TEvent>(sourcePeer, in _evt, receiveOrder));
        }

        public bool TryApplyToServer(NetworkPeerId sourcePeer, int receiveOrder) {
            return SW.SendEvent(new NetworkEventFromClient<TEvent>(sourcePeer, in _evt, receiveOrder));
        }
    }
}
