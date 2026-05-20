using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Buildings
{
    public readonly struct BuildingConstructionCompletedEvent : IEvent
    {
        public readonly EntityGID FinishedBuilding;
        public readonly BuildingId BuildingId;
        public readonly SettlementAnchorId AnchorId;
        public readonly ConstructionTransform Transform;

        public BuildingConstructionCompletedEvent(
            EntityGID finishedBuilding,
            BuildingId buildingId,
            SettlementAnchorId anchorId,
            in ConstructionTransform transform)
        {
            FinishedBuilding = finishedBuilding;
            BuildingId = buildingId;
            AnchorId = anchorId;
            Transform = transform;
        }
    }
}
