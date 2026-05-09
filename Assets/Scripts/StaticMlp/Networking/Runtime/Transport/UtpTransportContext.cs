using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;
using Unity.Collections;
using Unity.Jobs;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Error;
using Unity.Networking.Transport.Utilities;
using UnityEngine;

namespace StaticMlp.Networking.Transport {
    public sealed class UtpTransportContext : IDisposable, INetworkTransport, IResource {
        private const int SEND_SKIPPED_NO_CONNECTION = int.MinValue;
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
        public int DebugPingLatencyMs { get; private set; }

        public readonly Dictionary<ushort, NetworkConnection> ConnectionByPeer = new();
        public readonly Dictionary<NetworkConnection, NetworkPeerId> PeerByConnection = new();
        public ushort NextPeerId = 1;
        private readonly List<OutgoingPacket> _pendingReliablePackets = new();
        private readonly HashSet<ushort> _blockedReliablePeers = new();
        private SimulatorUtility.Parameters _simulatorParameters;

        public void Send(NetworkPeerId peer, ReadOnlySpan<byte> payload, NetDelivery delivery) {
            if (delivery == NetDelivery.ReliableSequenced && HasPendingReliablePacket(peer)) {
                EnqueueReliableRetry(peer, payload.ToArray());
                return;
            }

            var sendResult = TrySendImmediate(peer, payload, delivery);
            if (sendResult == (int)StatusCode.NetworkSendQueueFull && delivery == NetDelivery.ReliableSequenced) {
                EnqueueReliableRetry(peer, payload.ToArray());
                return;
            }

            if (sendResult == SEND_SKIPPED_NO_CONNECTION) {
                LogWarning($"Send skipped: no connection for peer {peer}, bytes={payload.Length}, delivery={delivery}");
                return;
            }

            if (sendResult < 0)
                LogWarning($"Send failed: peer={peer}, bytes={payload.Length}, delivery={delivery}, code={sendResult}");
        }

        public void FlushPendingReliablePackets() {
            if (_pendingReliablePackets.Count == 0)
                return;

            _blockedReliablePeers.Clear();

            for (var i = 0; i < _pendingReliablePackets.Count;) {
                var packet = _pendingReliablePackets[i];
                if (_blockedReliablePeers.Contains(packet.Peer.Value)) {
                    i++;
                    continue;
                }

                var sendResult = TrySendImmediate(packet.Peer, packet.Payload, packet.Delivery);
                if (sendResult == 0) {
                    _pendingReliablePackets.RemoveAt(i);
                    continue;
                }

                if (sendResult == (int)StatusCode.NetworkSendQueueFull) {
                    _blockedReliablePeers.Add(packet.Peer.Value);
                    i++;
                    continue;
                }

                if (sendResult == SEND_SKIPPED_NO_CONNECTION) {
                    LogWarning($"Dropping queued reliable packet because connection is no longer available: peer={packet.Peer}, bytes={packet.Payload.Length}");
                    _pendingReliablePackets.RemoveAt(i);
                    continue;
                }

                LogWarning($"Dropping queued reliable packet after send failure: peer={packet.Peer}, bytes={packet.Payload.Length}, code={sendResult}");
                _pendingReliablePackets.RemoveAt(i);
            }
        }

        private int TrySendImmediate(NetworkPeerId peer, ReadOnlySpan<byte> payload, NetDelivery delivery) {
            if (!TryGetConnection(peer, out var connection) || !connection.IsCreated) {
                return SEND_SKIPPED_NO_CONNECTION;
            }

            var pipeline = PipelineFor(delivery);
            var begin = Driver.BeginSend(pipeline, connection, out var writer, payload.Length);
            if (begin != 0)
                return begin;

            var payloadBytes = payload.ToArray();
            writer.WriteBytes(payloadBytes.AsSpan());
            return Driver.EndSend(writer);
        }

        private bool HasPendingReliablePacket(NetworkPeerId peer) {
            foreach (var packet in _pendingReliablePackets) {
                if (packet.Peer == peer)
                    return true;
            }

            return false;
        }

        private void EnqueueReliableRetry(NetworkPeerId peer, byte[] payload) {
            _pendingReliablePackets.Add(new OutgoingPacket {
                Peer = peer,
                Delivery = NetDelivery.ReliableSequenced,
                Payload = payload
            });
            Log($"Queued reliable packet for retry: peer={peer}, bytes={payload.Length}, pending={_pendingReliablePackets.Count}");
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
            _pendingReliablePackets.Clear();
            _blockedReliablePeers.Clear();
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

        public void SetSimulatorParameters(SimulatorUtility.Parameters simulatorParameters) {
            _simulatorParameters = simulatorParameters;
            DebugPingLatencyMs = simulatorParameters.PacketDelayMs * 2;
        }

        public void ConfigureDebugPingLatency(int pingLatencyMs) {
            var clampedPingLatencyMs = Mathf.Max(0, pingLatencyMs);
            DebugPingLatencyMs = clampedPingLatencyMs;
            _simulatorParameters.PacketDelayMs = clampedPingLatencyMs / 2;
            Driver.ModifySimulatorStageParameters(_simulatorParameters);
            Log($"Configured UTP debug ping latency: rtt={DebugPingLatencyMs}ms, outboundDelay={_simulatorParameters.PacketDelayMs}ms");
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
