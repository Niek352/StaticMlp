using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    public static class ServerConstructionAuthorization
    {
        public static bool HasPlayer(NetworkPeerId peer)
        {
            return TryGetPlayer(peer, out _);
        }

        public static bool TryGetPlayer(NetworkPeerId peer, out SW.Entity player)
        {
            foreach (var e in SW.Query<All<PlayerTag, NetworkIdentity>>().Entities())
            {
                ref readonly var identity = ref e.Read<NetworkIdentity>();
                if (identity.Owner != peer)
                    continue;

                player = e;
                return true;
            }

            player = default;
            return false;
        }

        public static bool OwnsSite(SW.Entity site, NetworkPeerId peer)
        {
            if (!site.Has<NetworkIdentity>())
                return false;

            ref readonly var identity = ref site.Read<NetworkIdentity>();
            return identity.Owner == peer;
        }

        public static bool IsPlayerNear(NetworkPeerId peer, Vector3 position, float maxDistance)
        {
            if (!TryGetPlayer(peer, out var player) || !player.Has<CharacterNetState>())
                return false;

            var playerPosition = player.Read<CharacterNetState>().Position;
            return (playerPosition - position).sqrMagnitude <= maxDistance * maxDistance;
        }
    }
}
