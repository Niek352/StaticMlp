using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.Player
{
    public static class PlayerSpawns
    {
        public static EntityGID SpawnPlayer(NetworkPeerId owner, Vector3 spawnPosition)
        {
            return NetworkEntitySpawner.SpawnServerEntity<PlayerNetworkEntity>(
                owner,
                NetworkAuthority.Owner,
                PlayerGameplayFeature.PLAYER,
                entity =>
                {
                    entity.Set<PlayerTag>();
                    entity.Set(new CharacterNetState
                    {
                        Position = spawnPosition,
                        Velocity = Vector3.zero,
                        Rotation = Quaternion.identity
                    });
                });
        }
    }
}
