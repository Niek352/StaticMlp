using StaticMlp.Networking.Replication;
using Steamworks;
using UnityEngine;

namespace StaticMlp.Networking.Transport {
    public static class SteamTransportStartup {
        public static SteamTransportContext StartServer(int virtualPort) {
            var ctx = new SteamTransportContext {
                IsServer = true,
                LocalPeerId = new NetworkPeerId(0),
                VirtualPort = virtualPort
            };

            // Relay sockets use the interface callback path to avoid missing OnMessage
            // callbacks that have been reported with derived SocketManager handlers.
            var socketManager = SteamNetworkingSockets.CreateRelaySocket<SocketManager>(virtualPort);
            socketManager.Interface = new SteamRelaySocketManager {
                Context = ctx
            };
            ctx.ServerSocketManager = socketManager;

            NetworkRuntime.LocalPeerId = ctx.LocalPeerId;
            ServerPeerRegistry.Clear();
            ServerDisconnectedPeerQueue.Clear();

            SW.SetResource(ctx);
            SW.SetResource(new NetInbox());
            SW.SetResource(new NetOutbox());
            SW.SetResource(new ServerChunkLeaseStore());
            SteamTransportContext.Log($"Steam server transport started on virtual port {virtualPort}");
            return ctx;
        }

        public static SteamTransportContext StartClient(SteamId hostSteamId, int virtualPort) {
            var ctx = new SteamTransportContext {
                IsServer = false,
                LocalPeerId = default,
                VirtualPort = virtualPort
            };

            var connectionManager = SteamNetworkingSockets.ConnectRelay<ConnectionManager>(hostSteamId, virtualPort);
            connectionManager.Interface = new SteamRelayConnectionManager {
                Context = ctx
            };
            ctx.ClientConnectionManager = connectionManager;

            CW.SetResource(ctx);
            CW.SetResource(new NetInbox());
            CW.SetResource(new NetOutbox());
            CW.SetResource(new ClientLocalChunkLease());
            SteamTransportContext.Log($"Steam client transport connecting to steamId={(ulong)hostSteamId} on virtual port {virtualPort}");
            return ctx;
        }

        public static void ConfigureDebugPingLatency(int pingLatencyMs) {
            var clampedPingLatencyMs = Mathf.Max(0, pingLatencyMs);
            var outboundDelayMs = clampedPingLatencyMs / 2;
            SteamNetworkingUtils.FakeSendPacketLag = outboundDelayMs;
            SteamNetworkingUtils.FakeRecvPacketLag = 0;
            SteamTransportContext.Log($"Configured Steam debug ping latency: rtt={clampedPingLatencyMs}ms, outboundDelay={outboundDelayMs}ms");
        }
    }
}
