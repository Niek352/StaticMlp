using System;
using Steamworks;
using Steamworks.Data;

namespace StaticMlp.Networking.Transport {
    public sealed class SteamRelaySocketManager : ISocketManager {
        public SteamTransportContext Context { get; set; }

        public void OnConnecting(Connection connection, ConnectionInfo info) {
            var result = connection.Accept();
            SteamTransportContext.Log($"Steam incoming connection accepted: steamId={(ulong)info.Identity.SteamId}, result={result}");
        }

        public void OnConnected(Connection connection, ConnectionInfo info) {
            Context?.HandleServerConnected(connection, info);
        }

        public void OnDisconnected(Connection connection, ConnectionInfo info) {
            Context?.HandleServerDisconnected(connection, info);
        }

        public void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel) {
            Context?.EnqueueServerMessage(connection, identity, data, size);
        }
    }
}
