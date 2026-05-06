using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace StaticMlp.Networking.Transport {
    public sealed unsafe class SteamTransportContext : IDisposable, INetworkTransport, IResource {
        public static bool EnableLogs = true;

        public readonly Queue<RawNetworkPacket> RawInbox = new();
        public readonly Dictionary<NetworkPeerId, Connection> ConnectionByPeer = new();
        public readonly Dictionary<uint, NetworkPeerId> PeerByConnectionId = new();
        public readonly Dictionary<ulong, NetworkPeerId> PeerBySteamId = new();
        public readonly Dictionary<ushort, ulong> SteamIdByPeer = new();

        public bool IsServer { get; set; }
        public NetworkPeerId LocalPeerId;
        public SocketManager ServerSocketManager;
        public ConnectionManager ClientConnectionManager;
        public ushort NextPeerId = 1;
        public int VirtualPort;

        public void Send(NetworkPeerId peer, ReadOnlySpan<byte> payload, NetDelivery delivery) {
            if (!TryGetConnection(peer, out var connection)) {
                LogWarning($"Steam send skipped: no connection for peer {peer}, bytes={payload.Length}, delivery={delivery}");
                return;
            }

            fixed (byte* payloadPtr = payload) {
                var result = connection.SendMessage((IntPtr)payloadPtr, payload.Length, SendTypeFor(delivery), 0);
                if (result != Result.OK)
                    LogWarning($"Steam send failed: peer={peer}, bytes={payload.Length}, delivery={delivery}, result={result}");
            }
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
            SteamClient.RunCallbacks();
            ServerSocketManager?.Receive();
            ClientConnectionManager?.Receive();
        }

        public void Dispose() {
            Log("Disposing Steam transport context");
            RawInbox.Clear();
            ConnectionByPeer.Clear();
            PeerByConnectionId.Clear();
            PeerBySteamId.Clear();
            SteamIdByPeer.Clear();

            if (ClientConnectionManager != null) {
                ClientConnectionManager.Close(false, 0, "Shutdown");
                ClientConnectionManager = null;
            }

            if (ServerSocketManager != null) {
                ServerSocketManager.Close();
                ServerSocketManager = null;
            }

            LocalPeerId = default;
            NetworkRuntime.LocalPeerId = default;
        }

        public void HandleServerConnected(Connection connection, ConnectionInfo info) {
            var steamId = (ulong)info.Identity.SteamId;
            if (steamId == 0) {
                LogWarning("Steam server connected callback without remote SteamId");
                return;
            }

            var peer = new NetworkPeerId(NextPeerId++);
            ConnectionByPeer[peer] = connection;
            PeerByConnectionId[connection.Id] = peer;
            PeerBySteamId[steamId] = peer;
            SteamIdByPeer[peer.Value] = steamId;
            ServerPeerRegistry.Add(peer);

            Log($"Steam peer connected: peer={peer}, steamId={steamId}");
            Send(peer, PacketCodec.EncodeWelcome(peer), NetDelivery.ReliableSequenced);
            ReplicationSnapshotBroadcaster.SendClusterSnapshot(peer, 0);
        }

        public void HandleServerDisconnected(Connection connection, ConnectionInfo info) {
            if (!PeerByConnectionId.TryGetValue(connection.Id, out var peer))
                return;

            Log($"Steam peer disconnected: peer={peer}, reason={info.EndReason}");
            RemovePeer(peer, connection.Id);
            ServerPeerRegistry.Remove(peer);
            ServerDisconnectedPeerQueue.Enqueue(peer);
        }

        public void HandleClientConnected() {
            Log("Steam client connected to host; sending hello");
            Send(new NetworkPeerId(0), PacketCodec.EncodeHello(), NetDelivery.ReliableSequenced);
        }

        public void HandleClientDisconnected(ConnectionInfo info) {
            Log($"Steam client disconnected: reason={info.EndReason}");
            LocalPeerId = default;
            NetworkRuntime.LocalPeerId = default;
        }

        public void EnqueueServerMessage(Connection connection, NetIdentity identity, IntPtr data, int size) {
            if (!PeerByConnectionId.TryGetValue(connection.Id, out var peer)) {
                var steamId = (ulong)identity.SteamId;
                if (steamId == 0 || !PeerBySteamId.TryGetValue(steamId, out peer))
                    return;
            }

            var payload = CopyPayload(data, size);
            RawInbox.Enqueue(new RawNetworkPacket(peer, payload));
        }

        public void EnqueueClientMessage(IntPtr data, int size) {
            var payload = CopyPayload(data, size);
            RawInbox.Enqueue(new RawNetworkPacket(new NetworkPeerId(0), payload));
        }

        public bool TryGetConnection(NetworkPeerId peer, out Connection connection) {
            if (IsServer)
                return ConnectionByPeer.TryGetValue(peer, out connection);

            if (ClientConnectionManager != null) {
                connection = ClientConnectionManager.Connection;
                return connection.Id != 0;
            }

            connection = default;
            return false;
        }

        private void RemovePeer(NetworkPeerId peer, uint connectionId) {
            ConnectionByPeer.Remove(peer);
            PeerByConnectionId.Remove(connectionId);

            if (SteamIdByPeer.Remove(peer.Value, out var steamId))
                PeerBySteamId.Remove(steamId);
        }

        private static SendType SendTypeFor(NetDelivery delivery) {
            return delivery switch {
                NetDelivery.ReliableSequenced => SendType.Reliable,
                NetDelivery.UnreliableSequenced => SendType.Unreliable,
                _ => SendType.Unreliable
            };
        }

        private static byte[] CopyPayload(IntPtr data, int size) {
            var payload = new byte[size];
            if (size > 0)
                Marshal.Copy(data, payload, 0, size);
            return payload;
        }

        public static void Log(string message) {
            if (EnableLogs)
                Debug.Log($"[StaticMlpSteamTransport] {message}");
        }

        public static void LogWarning(string message) {
            if (EnableLogs)
                Debug.LogWarning($"[StaticMlpSteamTransport] {message}");
        }
    }
}
