using StaticMlp.Features.Settlement;
using UnityEngine;

namespace StaticMlp.Features.CampFlow
{
    public readonly struct CampFlowAnchorSpawnSpec
    {
        public readonly SettlementAnchorId AnchorId;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly uint StartingFlagsMask;

        public CampFlowAnchorSpawnSpec(
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
