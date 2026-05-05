using System;
using Steamworks;
using Steamworks.Data;

namespace StaticMlp.Networking.Transport {
    public sealed class SteamRelayConnectionManager : ConnectionManager {
        public SteamTransportContext Context { get; set; }

        public override void OnConnected(ConnectionInfo info) {
            Context?.HandleClientConnected();
        }

        public override void OnDisconnected(ConnectionInfo info) {
            Context?.HandleClientDisconnected(info);
        }

        public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel) {
            Context?.EnqueueClientMessage(data, size);
        }
    }
}
