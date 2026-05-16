using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Transport;

namespace StaticMlp.Networking.Replication {
    public static class SpawnBroadcaster {
        public static void SendSpawn(SW.Entity entity) {
            if (!SW.IsWorldInitialized || !SW.HasResource<NetOutbox>())
                return;

            ref var outbox = ref SW.GetResource<NetOutbox>();
            foreach (var peer in ServerPeerRegistry.Peers)
                outbox.Enqueue(peer, PacketCodec.EncodeSpawn(CreateSpawn(entity, peer)), NetDelivery.ReliableSequenced);
        }

        public static void SendExistingSpawns(NetworkPeerId peer) {
            if (!SW.IsWorldInitialized || !SW.HasResource<NetOutbox>())
                return;

            ReadOnlySpan<ushort> clusters = stackalloc ushort[] { 0 };
            foreach (var e in SW.Query<All<NetworkedTag, NetworkIdentity>>().Entities(clusters: clusters))
                SendSpawn(e, peer);
        }

        public static void SendSpawn(SW.Entity entity, NetworkPeerId peer) {
            if (!SW.IsWorldInitialized || !SW.HasResource<NetOutbox>())
                return;

            ref var outbox = ref SW.GetResource<NetOutbox>();
            var spawn = CreateSpawn(entity, peer);
            outbox.Enqueue(peer, PacketCodec.EncodeSpawn(spawn), NetDelivery.ReliableSequenced);
        }

        private static SpawnMessage CreateSpawn(SW.Entity entity, NetworkPeerId targetPeer) {
            ref readonly var identity = ref entity.Read<NetworkIdentity>();
            var spawn = new SpawnMessage {
                Gid = entity.GID,
                EntityType = entity.EntityType,
                NetworkSchemaVersion = ReplicationRegistry.GetNetworkSchemaVersion(entity.EntityType),
                Owner = identity.Owner,
                Authority = identity.Authority,
                NetworkArchetypeId = identity.NetworkArchetypeId
            };

            spawn.SnapshotPayload = ReplicationRegistry.CreateInitialStateSnapshot(entity, targetPeer);
            return spawn;
        }
    }
}
