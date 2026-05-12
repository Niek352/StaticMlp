using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace StaticMlp.Features.Settlement
{
    public struct SettlementAnchorLocation : IComponent
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public SettlementAnchorLocation(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }
}
