using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Diagnostics;
using StaticMlp.Networking.Replication;
using Unity.Collections;
using Unity.Networking.Transport;

namespace StaticMlp.Networking.Transport {
    public sealed class ClientRawInboxDrainSystem : ISystem {
        public void Update() {
            ref var ctx = ref CW.GetResource<UtpTransportContext>();
            ref var inbox = ref CW.GetResource<NetInbox>();
            inbox.Clear();

            NetworkConnection connection;
            DataStreamReader reader;
            NetworkEvent.Type evt;

            while ((evt = ctx.Driver.PopEvent(out connection, out reader)) != NetworkEvent.Type.Empty) {
                if (evt == NetworkEvent.Type.Data) {
                    var payload = new byte[reader.Length];
                    reader.ReadBytes(payload.AsSpan());
                    NetworkTrafficProfiler.RecordReceived(new NetworkPeerId(0), payload);
                    ctx.RawInbox.Enqueue(new RawNetworkPacket(new NetworkPeerId(0), payload));
                } else if (evt == NetworkEvent.Type.Connect) {
                    UtpTransportContext.Log("Client connected to server; sending hello");
                    ctx.Send(new NetworkPeerId(0), PacketCodec.EncodeHello(), NetDelivery.ReliableSequenced);
                } else if (evt == NetworkEvent.Type.Disconnect) {
                    UtpTransportContext.Log("Client disconnected from server");
                    if (ctx.ServerConnection.IsCreated && ctx.ServerConnection.Length > 0)
                        ctx.ServerConnection[0] = default;
                    ctx.LocalPeerId = default;
                    NetworkRuntime.LocalPeerId = default;
                }
            }

            while (ctx.RawInbox.Count > 0) {
                var packet = ctx.RawInbox.Dequeue();
                PacketCodec.Decode(packet.SourcePeer, packet.Payload, inbox);
                var oldPeer = ctx.LocalPeerId;
                ctx.LocalPeerId = NetworkRuntime.LocalPeerId;
                if (oldPeer != ctx.LocalPeerId)
                    UtpTransportContext.Log($"Client local peer id set to {ctx.LocalPeerId}");
            }
        }
    }
}
