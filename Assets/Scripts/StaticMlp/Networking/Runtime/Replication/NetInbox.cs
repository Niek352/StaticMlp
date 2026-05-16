using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public sealed class NetInbox : IResource {
        public readonly List<SpawnMessage> Spawns = new();
        public readonly List<DespawnMessage> Despawns = new();
        public readonly List<OwnershipChangedMessage> OwnershipChanges = new();
        public readonly List<ReplicationSnapshotMessage> Snapshots = new();
        public readonly List<ChunkLeaseMessage> ChunkLeases = new();
        public readonly List<EntitySnapshotBatch> EntitySnapshotBatches = new();
        internal readonly List<NetworkEventPacket> Events = new();

        public void Clear() {
            Spawns.Clear();
            Despawns.Clear();
            OwnershipChanges.Clear();
            Snapshots.Clear();
            ChunkLeases.Clear();
            EntitySnapshotBatches.Clear();
            Events.Clear();
        }
    }
}
