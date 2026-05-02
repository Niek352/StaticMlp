using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Diagnostics;
using StaticMlp.Networking.Replication;
using Unity.Collections;
using Unity.Networking.Transport;

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
                    NetworkTrafficProfiler.RecordReceived(peer, payload);
                    ctx.RawInbox.Enqueue(new RawNetworkPacket(peer, payload));
                } else if (evt == NetworkEvent.Type.Disconnect) {
                    UtpTransportContext.Log($"Peer {peer} disconnected");
                    ctx.ConnectionByPeer.Remove(peer.Value);
                    ctx.PeerByConnection.Remove(connection);
                    ServerPeerRegistry.Remove(peer);
                }
            }

            while (ctx.RawInbox.Count > 0) {
                var packet = ctx.RawInbox.Dequeue();
                PacketCodec.Decode(packet.SourcePeer, packet.Payload, inbox);
            }
        }
    }
}
