using System;
using StaticMlp.Networking.Replication;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Utilities;

namespace StaticMlp.Networking.Transport {
    public static class UtpTransportStartup {
        private const int SIMULATOR_MAX_PACKET_COUNT = 128;

        public static UtpTransportContext StartClient(string host, ushort port, int debugPingLatencyMs) {
            var driver = CreateDriver(debugPingLatencyMs, out var simulatorParameters);
            var ctx = new UtpTransportContext {
                IsServer = false,
                Driver = driver,
                ServerConnection = new NativeArray<NetworkConnection>(1, Allocator.Persistent),
                LocalPeerId = default
            };

            ctx.SetSimulatorParameters(simulatorParameters);
            CreatePipelines(ctx);
            UtpTransportContext.Log($"Client pipelines created; connecting to {host}:{port}");
            ctx.ServerConnection[0] = ctx.Driver.Connect(NetworkEndpoint.Parse(host, port));

            CW.SetResource(ctx);
            CW.SetResource(new NetInbox());
            CW.SetResource(new NetOutbox());
            CW.SetResource(new ClientLocalChunkLease());
            UtpTransportContext.Log("Client transport resources registered");
            return ctx;
        }

        public static UtpTransportContext StartServer(ushort port, int debugPingLatencyMs) {
            var driver = CreateDriver(debugPingLatencyMs, out var simulatorParameters);
            var ctx = new UtpTransportContext {
                IsServer = true,
                Driver = driver,
                ClientConnections = new NativeList<NetworkConnection>(16, Allocator.Persistent),
                LocalPeerId = new NetworkPeerId(0)
            };

            ctx.SetSimulatorParameters(simulatorParameters);
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
            SW.SetResource(new ServerChunkLeaseStore());
            UtpTransportContext.Log("Server transport resources registered");
            return ctx;
        }

        private static void CreatePipelines(UtpTransportContext ctx) {
            ctx.UnreliablePipeline = ctx.Driver.CreatePipeline(typeof(SimulatorPipelineStage));
            ctx.UnreliableSequencedPipeline = ctx.Driver.CreatePipeline(typeof(UnreliableSequencedPipelineStage), typeof(SimulatorPipelineStage));
            ctx.ReliableSequencedPipeline = ctx.Driver.CreatePipeline(typeof(ReliableSequencedPipelineStage), typeof(SimulatorPipelineStage));
            UtpTransportContext.Log("Created unreliable, unreliable sequenced, and reliable sequenced pipelines with simulator stage");
        }

        private static NetworkDriver CreateDriver(int debugPingLatencyMs, out SimulatorUtility.Parameters simulatorParameters) {
            var settings = new NetworkSettings();
            settings.WithSimulatorStageParameters(
                maxPacketCount: SIMULATOR_MAX_PACKET_COUNT,
                mode: ApplyMode.SentPacketsOnly,
                packetDelayMs: ClampDebugPingLatency(debugPingLatencyMs) / 2);

            simulatorParameters = settings.GetSimulatorStageParameters();
            return NetworkDriver.Create(settings);
        }

        private static int ClampDebugPingLatency(int debugPingLatencyMs) {
            return Math.Max(0, debugPingLatencyMs);
        }
    }
}
