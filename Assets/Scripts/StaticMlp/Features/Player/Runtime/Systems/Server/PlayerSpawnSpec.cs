using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Player
{
    public readonly struct PlayerFactoryData
    {
        public readonly NetworkPeerId Owner;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public PlayerFactoryData(NetworkPeerId owner, Vector3 position, Quaternion rotation)
        {
            Owner = owner;
            Position = position;
            Rotation = rotation;
        }
    }
}
