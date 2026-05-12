using StaticMlp.Features.Settlement;
using UnityEngine;

namespace StaticMlp.Features.Stage1
{
    public readonly struct Stage1CampAnchorSpawnSpec
    {
        public readonly SettlementAnchorId AnchorId;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly uint StartingFlagsMask;

        public Stage1CampAnchorSpawnSpec(
            SettlementAnchorId anchorId,
            Vector3 position,
            Quaternion rotation,
            uint startingFlagsMask)
        {
            AnchorId = anchorId;
            Position = position;
            Rotation = rotation;
            StartingFlagsMask = startingFlagsMask;
        }
    }
}
