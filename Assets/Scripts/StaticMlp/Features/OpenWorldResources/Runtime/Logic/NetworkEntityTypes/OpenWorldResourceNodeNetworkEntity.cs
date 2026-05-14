using FFS.Libraries.StaticEcs;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.OpenWorldResources
{
    [NetworkEntityManifest(typeof(OpenWorldResourceNodeState), typeof(OpenWorldResourceNodeTransform))]
    public struct OpenWorldResourceNodeNetworkEntity : INetworkEntityType
    {
        public byte Id() => 9;
        public ushort NetworkSchemaVersion() => 1;
        public ushort DefaultNetworkArchetypeId() => OpenWorldResourceNetworkArchetypeIds.ResourceNode;
    }
}
