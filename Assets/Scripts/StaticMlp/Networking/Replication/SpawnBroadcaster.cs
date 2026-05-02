using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Transport;

namespace StaticMlp.Networking.Replication {
    public static class SpawnBroadcaster {
        public static void SendSpawn(SW.Entity entity) {
            if (!SW.IsWorldInitialized || !SW.HasResource<NetOutbox>())
                return;

            ref readonly var identity = ref entity.Read<NetworkIdentity>();
            var spawn = new SpawnMessage {
                Gid = entity.GID,
                Owner = identity.Owner,
                Authority = identity.Authority,
                PrefabId = identity.PrefabId
            };

            spawn.Components.Add(ReplicationRegistry.CreateDelta(entity.GID, identity));
            if (entity.Has<CharacterNetState>())
                spawn.Components.Add(ReplicationRegistry.CreateDelta(entity.GID, entity.Read<CharacterNetState>()));

            ref var outbox = ref SW.GetResource<NetOutbox>();
            foreach (var peer in ServerPeerRegistry.Peers)
                outbox.Enqueue(peer, PacketCodec.EncodeSpawn(spawn), NetDelivery.ReliableSequenced);
        }
    }
}
