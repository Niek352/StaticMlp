using System;
using Steamworks;
using Steamworks.Data;

namespace StaticMlp.Networking.Transport {
    public sealed class SteamRelayConnectionManager : IConnectionManager {
        public SteamTransportContext Context { get; set; }

        public void OnConnected(ConnectionInfo info) {
            Context?.HandleClientConnected();
        }

        public void OnDisconnected(ConnectionInfo info) {
            Context?.HandleClientDisconnected(info);
        }

        public void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel) {
            Context?.EnqueueClientMessage(data, size);
        }

        public void OnConnecting(ConnectionInfo info) { }
    }
}
