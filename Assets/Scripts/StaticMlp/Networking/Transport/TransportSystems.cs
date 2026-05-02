using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;
using Unity.Collections;
using Unity.Networking.Transport;

namespace StaticMlp.Networking.Transport {
    public sealed class ServerTransportCompleteSystem : ISystem {
        public void Update() {
            if (SW.HasResource<UtpTransportContext>())
                SW.GetResource<UtpTransportContext>().TransportJobHandle.Complete();
        }
    }

    public sealed class ClientTransportCompleteSystem : ISystem {
        public void Update() {
            if (CW.HasResource<UtpTransportContext>())
                CW.GetResource<UtpTransportContext>().TransportJobHandle.Complete();
        }
    }

    public sealed class ServerConnectionLifecycleSystem : ISystem {
        public void Update() {
            ref var ctx = ref SW.GetResource<UtpTransportContext>();

            NetworkConnection connection;
            while ((connection = ctx.Driver.Accept()).IsCreated) {
                var peer = new NetworkPeerId(ctx.NextPeerId++);
                ctx.ClientConnections.Add(connection);
                ctx.ConnectionByPeer[peer.Value] = connection;
                ctx.PeerByConnection[connection] = peer;
                ServerPeerRegistry.Add(peer);
                UtpTransportContext.Log($"Accepted client connection as peer {peer}");
                ctx.Send(peer, PacketCodec.EncodeWelcome(peer), NetDelivery.ReliableSequenced);
                UtpTransportContext.Log($"Sent welcome to peer {peer}");
            }
        }
    }

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
                    ServerPeerRegistry.Remove(peer);
                }
            }

            while (ctx.RawInbox.Count > 0) {
                var packet = ctx.RawInbox.Dequeue();
                PacketCodec.Decode(packet.SourcePeer, packet.Payload, inbox);
            }
        }
    }

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
                    ctx.RawInbox.Enqueue(new RawNetworkPacket(new NetworkPeerId(0), payload));
                } else if (evt == NetworkEvent.Type.Connect) {
                    UtpTransportContext.Log("Client connected to server; sending hello");
                    ctx.Send(new NetworkPeerId(0), PacketCodec.EncodeHello(), NetDelivery.ReliableSequenced);
                } else if (evt == NetworkEvent.Type.Disconnect) {
                    UtpTransportContext.Log("Client disconnected from server");
                    if (ctx.ServerConnection.IsCreated && ctx.ServerConnection.Length > 0)
                        ctx.ServerConnection[0] = default;
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

    public sealed class ServerTransportSendSystem : ISystem {
        public void Update() {
            ref var ctx = ref SW.GetResource<UtpTransportContext>();
            ref var outbox = ref SW.GetResource<NetOutbox>();

            foreach (var packet in outbox.Packets)
                ctx.Send(packet.Peer, packet.Payload, packet.Delivery);

            outbox.Clear();
        }
    }

    public sealed class ClientTransportSendSystem : ISystem {
        public void Update() {
            ref var ctx = ref CW.GetResource<UtpTransportContext>();
            ref var outbox = ref CW.GetResource<NetOutbox>();

            foreach (var packet in outbox.Packets)
                ctx.Send(packet.Peer, packet.Payload, packet.Delivery);

            outbox.Clear();
        }
    }

    public sealed class ServerTransportScheduleSystem : ISystem {
        public void Update() {
            ref var ctx = ref SW.GetResource<UtpTransportContext>();
            ctx.TransportJobHandle = ctx.Driver.ScheduleUpdate();
        }
    }

    public sealed class ClientTransportScheduleSystem : ISystem {
        public void Update() {
            ref var ctx = ref CW.GetResource<UtpTransportContext>();
            ctx.TransportJobHandle = ctx.Driver.ScheduleUpdate();
        }
    }
}
