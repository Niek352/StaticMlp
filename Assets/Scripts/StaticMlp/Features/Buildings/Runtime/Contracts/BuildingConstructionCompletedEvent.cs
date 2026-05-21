using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Settlement;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    public readonly struct BuildingConstructionCompletedEvent : IEvent
    {
        public readonly EntityGID FinishedBuilding;
        public readonly BuildingId BuildingId;
        public readonly SettlementAnchorId AnchorId;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public BuildingConstructionCompletedEvent(
            EntityGID finishedBuilding,
            BuildingId buildingId,
            SettlementAnchorId anchorId,
            Vector3 position,
            Quaternion rotation)
        {
            FinishedBuilding = finishedBuilding;
            BuildingId = buildingId;
            AnchorId = anchorId;
            Position = position;
            Rotation = rotation;
        }
    }
}
