using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public sealed class NetInbox : IResource {
        public readonly List<SpawnMessage> Spawns = new();
        public readonly List<DespawnMessage> Despawns = new();
        public readonly List<OwnershipChangedMessage> OwnershipChanges = new();
        public readonly List<ComponentBatch> ComponentBatches = new();
        public readonly List<NetworkEventMessage> Events = new();

        public void Clear() {
            Spawns.Clear();
            Despawns.Clear();
            OwnershipChanges.Clear();
            ComponentBatches.Clear();
            Events.Clear();
        }
    }
}
