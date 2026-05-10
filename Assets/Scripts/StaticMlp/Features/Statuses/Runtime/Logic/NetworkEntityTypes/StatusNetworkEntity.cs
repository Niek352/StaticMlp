using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Statuses
{
    [NetworkEntityManifest(typeof(StatusTarget), typeof(StatusStrength), typeof(StatusContext))]
    public struct StatusNetworkEntity : INetworkEntityType
    {
        public byte Id() => 6;
        public ushort NetworkSchemaVersion() => 1;
        public ushort DefaultNetworkArchetypeId() => 0;
    }
}
