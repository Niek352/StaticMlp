using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Transport;

namespace StaticMlp.Networking.Replication {
    public static class DespawnBroadcaster {
        public static void SendDespawn(EntityGID gid) {
            if (!SW.IsWorldInitialized || !SW.HasResource<NetOutbox>())
                return;

            ref var outbox = ref SW.GetResource<NetOutbox>();
            var payload = PacketCodec.EncodeDespawn(new DespawnMessage(gid));

            foreach (var peer in ServerPeerRegistry.Peers)
                outbox.Enqueue(peer, payload, NetDelivery.ReliableSequenced);
        }
    }
}
