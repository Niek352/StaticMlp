using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Player
{
    public readonly struct PlayerSpawnSpec
    {
        public readonly NetworkPeerId Owner;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public PlayerSpawnSpec(NetworkPeerId owner, Vector3 position, Quaternion rotation)
        {
            Owner = owner;
            Position = position;
            Rotation = rotation;
        }
    }
}
