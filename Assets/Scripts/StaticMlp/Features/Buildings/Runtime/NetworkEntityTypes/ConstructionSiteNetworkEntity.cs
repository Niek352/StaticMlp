using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Buildings {
    [NetworkEntityManifest(
        typeof(SettlementAnchorRef),
        typeof(ConstructionSiteState),
        typeof(ConstructionTransform),
        typeof(ConstructionResources),
        typeof(ConstructionProgress))]
    public struct ConstructionSiteNetworkEntity : INetworkEntityType {
        public byte Id() => 3;
        public ushort NetworkSchemaVersion() => 1;
        public ushort DefaultNetworkArchetypeId() => BuildingNetworkArchetypeIds.CampCoreBlueprint;
    }
}
