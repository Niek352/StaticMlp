using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;
using Unity.Networking.Transport;

namespace StaticMlp.Networking.Transport {
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
                var lease = SW.GetResource<ServerChunkLeaseStore>().GetOrCreate(peer);
                ctx.Send(peer, PacketCodec.EncodeChunkLease(new ChunkLeaseMessage(lease)), NetDelivery.ReliableSequenced);
                UtpTransportContext.Log($"Sent chunk lease to peer {peer}");
            }
        }
    }
}
