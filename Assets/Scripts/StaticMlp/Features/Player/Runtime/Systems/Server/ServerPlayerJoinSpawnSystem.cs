using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Transport;
using UnityEngine;

namespace StaticMlp.Features.Player
{
    public sealed class ServerPlayerJoinSpawnSystem : ISystem
    {
        private readonly float _spawnSpacing;

        public ServerPlayerJoinSpawnSystem(float spawnSpacing = 2.5f)
        {
            _spawnSpacing = spawnSpacing;
        }

        public void Update()
        {
            for (var i = 0; i < ServerPeerRegistry.Peers.Count; i++)
            {
                var peer = ServerPeerRegistry.Peers[i];
                if (HasPlayer(peer))
                    continue;

                Debug.Log($"[ServerPlayerJoinSpawn] Spawning player for peer={peer.Value}");
                var spawnPosition = new Vector3((peer.Value - 1) * _spawnSpacing, 0f, 0f);
                SW.GetResource<PlayerFactory>().Spawn(new PlayerFactoryData(peer, spawnPosition, Quaternion.identity));
                SpawnBroadcaster.SendExistingSpawns(peer);
            }
        }

        private static bool HasPlayer(NetworkPeerId peer)
        {
            foreach (var e in SW.Query<All<PlayerTag, NetworkIdentity>>().Entities())
            {
                ref readonly var identity = ref e.Read<NetworkIdentity>();
                if (identity.Owner == peer)
                    return true;
            }

            return false;
        }
    }
}
