using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public sealed class SpawnMessage {
        public EntityGID Gid;
        public byte EntityType;
        public ushort NetworkSchemaVersion;
        public NetworkPeerId Owner;
        public NetworkAuthority Authority;
        public ushort NetworkArchetypeId;
        public readonly List<ComponentDelta> Components = new();
    }
}
