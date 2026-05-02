using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public sealed class SpawnMessage {
        public EntityGID Gid;
        public NetworkPeerId Owner;
        public NetworkAuthority Authority;
        public ushort PrefabId;
        public readonly List<ComponentDelta> Components = new();
    }
}
