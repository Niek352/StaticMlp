using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    [Obsolete("Temp")]
    internal sealed class LegacyNetworkEventPayload<TEvent> : INetworkEventPayload
        where TEvent : struct, IEvent {
        private readonly TEvent _evt;
        private readonly NetworkEventRegistry.LegacyWriter<TEvent> _writer;

        public LegacyNetworkEventPayload(NetworkEventRegistry.LegacyWriter<TEvent> writer, in TEvent evt) {
            _writer = writer;
            _evt = evt;
        }

        public void Write(ref NetworkWriter writer) {
            var bytes = _writer(in _evt);
            if (bytes != null)
                writer.WriteBytes(bytes);
        }

        public bool TryApplyToClient(NetworkPeerId sourcePeer, int receiveOrder) {
            return CW.SendEvent(new NetworkEventFromServer<TEvent>(sourcePeer, in _evt, receiveOrder));
        }

        public bool TryApplyToServer(NetworkPeerId sourcePeer, int receiveOrder) {
            return SW.SendEvent(new NetworkEventFromClient<TEvent>(sourcePeer, in _evt, receiveOrder));
        }
    }
}
