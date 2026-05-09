using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Combat;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.Player
{
    public static class PlayerSpawns
    {
        private const float DEFAULT_MAX_HEALTH = 100f;

        public static EntityGID SpawnPlayer(NetworkPeerId owner, Vector3 spawnPosition)
        {
            return NetworkEntitySpawner.SpawnServerEntity<PlayerNetworkEntity>(
                owner,
                NetworkAuthority.Owner,
                PlayerGameplayFeature.PLAYER,
                entity =>
                {
                    entity.Set<PlayerTag>();
                    entity.Set(new ServerCombatAttackState());
                    entity.Set(new Health
                    {
                        Current = DEFAULT_MAX_HEALTH,
                        Max = DEFAULT_MAX_HEALTH
                    });
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
