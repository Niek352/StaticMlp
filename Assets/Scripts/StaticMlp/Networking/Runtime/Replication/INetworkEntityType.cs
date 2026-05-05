using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public interface INetworkEntityType : IEntityType {
        ushort NetworkSchemaVersion();
        ushort DefaultNetworkArchetypeId();
    }
}
