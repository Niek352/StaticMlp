using StaticMlp.Networking.Replication;
using Steamworks;

namespace StaticMlp.Networking.Transport {
    public static class SteamTransportStartup {
        public static SteamTransportContext StartServer(int virtualPort) {
            var ctx = new SteamTransportContext {
                IsServer = true,
                LocalPeerId = new NetworkPeerId(0),
                VirtualPort = virtualPort
            };

            var socketManager = SteamNetworkingSockets.CreateRelaySocket<SteamRelaySocketManager>(virtualPort);
            socketManager.Context = ctx;
            ctx.ServerSocketManager = socketManager;

            NetworkRuntime.LocalPeerId = ctx.LocalPeerId;
            ServerPeerRegistry.Clear();
            ServerDisconnectedPeerQueue.Clear();

            SW.SetResource(ctx);
            SW.SetResource(new NetInbox());
            SW.SetResource(new NetOutbox());
            SteamTransportContext.Log($"Steam server transport started on virtual port {virtualPort}");
            return ctx;
        }

        public static SteamTransportContext StartClient(SteamId hostSteamId, int virtualPort) {
            var ctx = new SteamTransportContext {
                IsServer = false,
                LocalPeerId = default,
                VirtualPort = virtualPort
            };

            var connectionManager = SteamNetworkingSockets.ConnectRelay<SteamRelayConnectionManager>(hostSteamId, virtualPort);
            connectionManager.Context = ctx;
            ctx.ClientConnectionManager = connectionManager;

            CW.SetResource(ctx);
            CW.SetResource(new NetInbox());
            CW.SetResource(new NetOutbox());
            SteamTransportContext.Log($"Steam client transport connecting to steamId={(ulong)hostSteamId} on virtual port {virtualPort}");
            return ctx;
        }
    }
}
