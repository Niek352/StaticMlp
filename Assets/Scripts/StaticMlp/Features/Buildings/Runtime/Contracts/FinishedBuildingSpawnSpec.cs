using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    public readonly struct FinishedBuildingSpawnSpec
    {
        public readonly NetworkPeerId Owner;
        public readonly BuildingDefinition Definition;
        public readonly SettlementAnchorId AnchorId;
        public readonly ConstructionTransform Transform;

        public FinishedBuildingSpawnSpec(
            NetworkPeerId owner,
            in BuildingDefinition definition,
            SettlementAnchorId anchorId,
            in ConstructionTransform transform)
        {
            Owner = owner;
            Definition = definition;
            AnchorId = anchorId;
            Transform = transform;
        }
    }
}
