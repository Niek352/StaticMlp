using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    [NetworkEntityManifest(typeof(SettlementSharedResources))]
    public struct SettlementSharedResourcesNetworkEntity : INetworkEntityType
    {
        public byte Id() => 7;
        public ushort NetworkSchemaVersion() => 1;
        public ushort DefaultNetworkArchetypeId() => SettlementNetworkArchetypeIds.ResourceStorage;
    }
}
