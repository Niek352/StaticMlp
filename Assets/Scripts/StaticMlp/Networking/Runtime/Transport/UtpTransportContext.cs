using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Networking.Transport;
using UnityEngine;

namespace StaticMlp.Networking.Transport {
    public sealed class UtpTransportContext : IDisposable, INetworkTransport, IResource {
        public static bool EnableLogs = true;

        public NetworkDriver Driver;
        public NetworkPipeline UnreliablePipeline;
        public NetworkPipeline UnreliableSequencedPipeline;
        public NetworkPipeline ReliableSequencedPipeline;
        public NativeArray<NetworkConnection> ServerConnection;
        public NativeList<NetworkConnection> ClientConnections;
        public JobHandle TransportJobHandle;
        public readonly Queue<RawNetworkPacket> RawInbox = new();
        public bool IsServer { get; set; }
        public NetworkPeerId LocalPeerId;

        public readonly Dictionary<ushort, NetworkConnection> ConnectionByPeer = new();
        public readonly Dictionary<NetworkConnection, NetworkPeerId> PeerByConnection = new();
        public ushort NextPeerId = 1;

        public void Send(NetworkPeerId peer, ReadOnlySpan<byte> payload, NetDelivery delivery) {
            if (!TryGetConnection(peer, out var connection) || !connection.IsCreated) {
                LogWarning($"Send skipped: no connection for peer {peer}, bytes={payload.Length}, delivery={delivery}");
                return;
            }

            var pipeline = PipelineFor(delivery);
            var begin = Driver.BeginSend(pipeline, connection, out var writer, payload.Length);
            if (begin != 0) {
                LogWarning($"BeginSend failed: peer={peer}, bytes={payload.Length}, delivery={delivery}, code={begin}");
                return;
            }

            var payloadBytes = payload.ToArray();
            writer.WriteBytes(payloadBytes.AsSpan());
            var end = Driver.EndSend(writer);
            if (end < 0)
                LogWarning($"EndSend failed: peer={peer}, bytes={payload.Length}, delivery={delivery}, code={end}");
        }

        public bool TryReceive(out NetworkPeerId peer, out ReadOnlySpan<byte> payload) {
            if (RawInbox.Count == 0) {
                peer = default;
                payload = default;
                return false;
            }

            var packet = RawInbox.Dequeue();
            peer = packet.SourcePeer;
            payload = packet.Payload;
            return true;
        }

        public void Poll() {
            TransportJobHandle.Complete();
            TransportJobHandle = Driver.ScheduleUpdate();
        }

        public void Dispose() {
            Log("Disposing transport context");
            DisconnectActiveConnections();
            RawInbox.Clear();
            ConnectionByPeer.Clear();
            PeerByConnection.Clear();
            TransportJobHandle.Complete();
            if (ServerConnection.IsCreated)
                ServerConnection.Dispose();
            if (ClientConnections.IsCreated)
                ClientConnections.Dispose();
            if (Driver.IsCreated)
                Driver.Dispose();
        }

        public void DisconnectActiveConnections() {
            TransportJobHandle.Complete();

            if (!Driver.IsCreated)
                return;

            if (IsServer) {
                DisconnectClients();
                ServerPeerRegistry.Clear();
                ServerDisconnectedPeerQueue.Clear();
            } else {
                DisconnectServer();
            }

            Driver.ScheduleUpdate().Complete();
        }

        private void DisconnectServer() {
            if (!ServerConnection.IsCreated || ServerConnection.Length == 0)
                return;

            var connection = ServerConnection[0];
            if (connection.IsCreated)
                Driver.Disconnect(connection);

            ServerConnection[0] = default;
            LocalPeerId = default;
            NetworkRuntime.LocalPeerId = default;
        }

        private void DisconnectClients() {
            if (!ClientConnections.IsCreated)
                return;

            for (var i = 0; i < ClientConnections.Length; i++) {
                var connection = ClientConnections[i];
                if (connection.IsCreated)
                    Driver.Disconnect(connection);
            }

            ClientConnections.Clear();
        }

        public bool TryGetConnection(NetworkPeerId peer, out NetworkConnection connection) {
            if (IsServer)
                return ConnectionByPeer.TryGetValue(peer.Value, out connection);

            if (ServerConnection.IsCreated && ServerConnection.Length > 0) {
                connection = ServerConnection[0];
                return connection.IsCreated;
            }

            connection = default;
            return false;
        }

        public NetworkPipeline PipelineFor(NetDelivery delivery) {
            return delivery switch {
                NetDelivery.UnreliableSequenced => UnreliableSequencedPipeline,
                NetDelivery.ReliableSequenced => ReliableSequencedPipeline,
                _ => UnreliablePipeline
            };
        }

        public static void Log(string message) {
            if (EnableLogs)
                Debug.Log($"[StaticMlpTransport] {message}");
        }

        public static void LogWarning(string message) {
            if (EnableLogs)
                Debug.LogWarning($"[StaticMlpTransport] {message}");
        }
    }
}
