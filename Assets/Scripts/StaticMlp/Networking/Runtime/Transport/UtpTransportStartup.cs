using System;
using StaticMlp.Networking.Replication;
using Unity.Collections;
using Unity.Networking.Transport;

namespace StaticMlp.Networking.Transport {
    public static class UtpTransportStartup {
        public static UtpTransportContext StartClient(string host, ushort port) {
            var ctx = new UtpTransportContext {
                IsServer = false,
                Driver = NetworkDriver.Create(),
                ServerConnection = new NativeArray<NetworkConnection>(1, Allocator.Persistent),
                LocalPeerId = default
            };

            CreatePipelines(ctx);
            UtpTransportContext.Log($"Client pipelines created; connecting to {host}:{port}");
            ctx.ServerConnection[0] = ctx.Driver.Connect(NetworkEndpoint.Parse(host, port));

            CW.SetResource(ctx);
            CW.SetResource(new NetInbox());
            CW.SetResource(new NetOutbox());
            UtpTransportContext.Log("Client transport resources registered");
            return ctx;
        }

        public static UtpTransportContext StartServer(ushort port) {
            var ctx = new UtpTransportContext {
                IsServer = true,
                Driver = NetworkDriver.Create(),
                ClientConnections = new NativeList<NetworkConnection>(16, Allocator.Persistent),
                LocalPeerId = new NetworkPeerId(0)
            };

            CreatePipelines(ctx);
            var endpoint = NetworkEndpoint.AnyIpv4.WithPort(port);

            if (ctx.Driver.Bind(endpoint) != 0)
                throw new InvalidOperationException($"Failed to bind port {port}");

            ctx.Driver.Listen();
            UtpTransportContext.Log($"Server listening on port {port}");
            NetworkRuntime.LocalPeerId = ctx.LocalPeerId;
            ServerPeerRegistry.Clear();
            ServerDisconnectedPeerQueue.Clear();

            SW.SetResource(ctx);
            SW.SetResource(new NetInbox());
            SW.SetResource(new NetOutbox());
            UtpTransportContext.Log("Server transport resources registered");
            return ctx;
        }

        private static void CreatePipelines(UtpTransportContext ctx) {
            ctx.UnreliablePipeline = NetworkPipeline.Null;
            ctx.UnreliableSequencedPipeline = ctx.Driver.CreatePipeline(typeof(UnreliableSequencedPipelineStage));
            ctx.ReliableSequencedPipeline = ctx.Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage));
            UtpTransportContext.Log("Created unreliable, unreliable sequenced, and reliable sequenced pipelines");
        }
    }
}
