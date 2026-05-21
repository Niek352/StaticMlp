using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    public readonly struct FinishedBuildingSpawnSpec
    {
        public readonly NetworkPeerId Owner;
        public readonly BuildingDefinition Definition;
        public readonly SettlementAnchorId AnchorId;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public FinishedBuildingSpawnSpec(
            NetworkPeerId owner,
            in BuildingDefinition definition,
            SettlementAnchorId anchorId,
            Vector3 position,
            Quaternion rotation)
        {
            Owner = owner;
            Definition = definition;
            AnchorId = anchorId;
            Position = position;
            Rotation = rotation;
        }
    }
}
