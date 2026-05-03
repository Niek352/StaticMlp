using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Game.Systems.Server
{
    public static class ServerPeerPlayers
    {
        public static bool HasPlayer(NetworkPeerId peer)
        {
            return TryGetPlayer(peer, out _);
        }

        public static bool TryGetPlayer(NetworkPeerId peer, out SW.Entity player)
        {
            foreach (var entity in SW.Query<All<PlayerTag, NetworkIdentity>>().Entities())
            {
                if (entity.Read<NetworkIdentity>().Owner != peer)
                    continue;

                player = entity;
                return true;
            }

            player = default;
            return false;
        }

        public static bool TryGetPlayerPosition(NetworkPeerId peer, out Vector3 position)
        {
            if (TryGetPlayer(peer, out var player) && player.Has<CharacterNetState>())
            {
                position = player.Read<CharacterNetState>().Position;
                return true;
            }

            position = default;
            return false;
        }

        public static bool IsPlayerNear(NetworkPeerId peer, Vector3 position, float maxDistance)
        {
            return TryGetPlayerPosition(peer, out var playerPosition)
                   && (playerPosition - position).sqrMagnitude <= maxDistance * maxDistance;
        }
    }
}
