using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

namespace StaticMlp.Networking.Transport {
    public sealed class ServerRawInboxDrainSystem : ISystem {
        public void Update() {
            ref var ctx = ref SW.GetResource<UtpTransportContext>();
            ref var inbox = ref SW.GetResource<NetInbox>();
            inbox.Clear();

            NetworkConnection connection;
            DataStreamReader reader;
            NetworkEvent.Type evt;

            while ((evt = ctx.Driver.PopEvent(out connection, out reader)) != NetworkEvent.Type.Empty) {
                if (!ctx.PeerByConnection.TryGetValue(connection, out var peer))
                    continue;

                if (evt == NetworkEvent.Type.Data) {
                    var payload = new byte[reader.Length];
                    reader.ReadBytes(payload.AsSpan());
                    ctx.RawInbox.Enqueue(new RawNetworkPacket(peer, payload));
                } else if (evt == NetworkEvent.Type.Disconnect) {
                    UtpTransportContext.Log($"Peer {peer} disconnected");
                    ctx.ConnectionByPeer.Remove(peer.Value);
                    ctx.PeerByConnection.Remove(connection);
                    RemoveClientConnection(ref ctx, connection);
                    ServerPeerRegistry.Remove(peer);
                    ServerDisconnectedPeerQueue.Enqueue(peer);
                }
            }

            while (ctx.RawInbox.Count > 0) {
                var packet = ctx.RawInbox.Dequeue();
                var decoded = PacketCodec.Decode(packet.SourcePeer, packet.Payload, inbox);
                var typeStr = packet.Payload?.Length > 0 ? ((NetPacketType)packet.Payload[0]).ToString() : "Empty";
                Debug.Log($"[ServerRawInbox] Decoded packet from peer={packet.SourcePeer.Value} type={typeStr} decoded={decoded}");
            }
        }

        private static void RemoveClientConnection(ref UtpTransportContext ctx, NetworkConnection connection) {
            if (!ctx.ClientConnections.IsCreated)
                return;

            for (var i = 0; i < ctx.ClientConnections.Length; i++) {
                if (ctx.ClientConnections[i] != connection)
                    continue;

                ctx.ClientConnections.RemoveAtSwapBack(i);
                return;
            }
        }
    }
}
