using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Buildings {
    [NetworkEntityManifest(
        typeof(ConstructionSiteState),
        typeof(ConstructionTransform),
        typeof(ConstructionResources),
        typeof(ConstructionProgress))]
    public struct FinishedBuildingNetworkEntity : INetworkEntityType {
        public byte Id() => 4;
        public ushort NetworkSchemaVersion() => 1;
        public ushort DefaultNetworkArchetypeId() => BuildingNetworkArchetypeIds.WoodenHutFinished;
    }
}
