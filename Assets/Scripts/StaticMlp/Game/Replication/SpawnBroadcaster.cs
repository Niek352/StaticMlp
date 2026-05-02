using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Transport;

namespace StaticMlp.Networking.Replication {
    public static class SpawnBroadcaster {
        public static void SendSpawn(SW.Entity entity) {
            if (!SW.IsWorldInitialized || !SW.HasResource<NetOutbox>())
                return;

            var spawn = CreateSpawn(entity);

            ref var outbox = ref SW.GetResource<NetOutbox>();
            foreach (var peer in ServerPeerRegistry.Peers)
                outbox.Enqueue(peer, PacketCodec.EncodeSpawn(spawn), NetDelivery.ReliableSequenced);
        }

        public static void SendExistingSpawns(NetworkPeerId peer) {
            if (!SW.IsWorldInitialized || !SW.HasResource<NetOutbox>())
                return;

            foreach (var e in SW.Query<All<NetworkedTag, NetworkIdentity>>().Entities())
                SendSpawn(e, peer);
        }

        public static void SendSpawn(SW.Entity entity, NetworkPeerId peer) {
            if (!SW.IsWorldInitialized || !SW.HasResource<NetOutbox>())
                return;

            ref var outbox = ref SW.GetResource<NetOutbox>();
            outbox.Enqueue(peer, PacketCodec.EncodeSpawn(CreateSpawn(entity)), NetDelivery.ReliableSequenced);
        }

        private static SpawnMessage CreateSpawn(SW.Entity entity) {
            ref readonly var identity = ref entity.Read<NetworkIdentity>();
            var spawn = new SpawnMessage {
                Gid = entity.GID,
                Owner = identity.Owner,
                Authority = identity.Authority,
                PrefabId = identity.PrefabId
            };

            ReplicationRegistry.CollectInitialState(entity, spawn.Components);
            return spawn;
        }
    }
}
