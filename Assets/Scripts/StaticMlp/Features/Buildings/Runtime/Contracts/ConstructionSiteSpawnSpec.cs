using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    public readonly struct ConstructionSiteSpawnSpec
    {
        public readonly NetworkPeerId Owner;
        public readonly BuildingDefinition Definition;
        public readonly SettlementAnchorId AnchorId;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly bool StartReadyToBuild;
        public readonly float InitialBuildWork;

        public ConstructionSiteSpawnSpec(
            NetworkPeerId owner,
            in BuildingDefinition definition,
            SettlementAnchorId anchorId,
            Vector3 position,
            Quaternion rotation,
            bool startReadyToBuild,
            float initialBuildWork)
        {
            Owner = owner;
            Definition = definition;
            AnchorId = anchorId;
            Position = position;
            Rotation = rotation;
            StartReadyToBuild = startReadyToBuild;
            InitialBuildWork = initialBuildWork;
        }
    }
}
