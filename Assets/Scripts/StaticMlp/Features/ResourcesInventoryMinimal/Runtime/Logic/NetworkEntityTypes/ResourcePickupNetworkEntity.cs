using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    [NetworkEntityManifest(typeof(ResourcePickup))]
    public struct ResourcePickupNetworkEntity : INetworkEntityType
    {
        public byte Id() => 11;
        public ushort NetworkSchemaVersion() => 1;
        public ushort DefaultNetworkArchetypeId() => ResourcesInventoryMinimalGameplayFeature.RESOURCE_PICKUP;
    }
}
