using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;

namespace StaticMlp.Features.Player
{
    public static class ClientLocalPlayer
    {
        public static bool TryGetPosition(out Vector3 position)
        {
            foreach (var entity in CW.Query<All<LocalOwned, PlayerTag, CharacterNetState>>().Entities())
            {
                position = entity.Read<CharacterNetState>().Position;
                return true;
            }

            position = default;
            return false;
        }
    }
}
