using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Transport;

namespace StaticMlp.Game.Systems.Server {
    public sealed class ServerPeerDisconnectCleanupSystem : ISystem {
        private readonly List<EntityGID> _ownedEntities = new();

        public void Update() {
            while (ServerDisconnectedPeerQueue.TryDequeue(out var peer))
                CleanupPeer(peer);
        }

        private void CleanupPeer(NetworkPeerId peer) {
            _ownedEntities.Clear();

            foreach (var e in SW.Query<All<NetworkedTag, NetworkIdentity>>().Entities()) {
                ref readonly var identity = ref e.Read<NetworkIdentity>();
                if (identity.Owner == peer)
                    _ownedEntities.Add(e.GID);
            }

            foreach (var gid in _ownedEntities) {
                DespawnBroadcaster.SendDespawn(gid);

                if (gid.TryUnpack<ServerWT>(out var e))
                    e.Destroy();
            }

            _ownedEntities.Clear();
        }
    }
}
