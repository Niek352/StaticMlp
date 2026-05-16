using System.Collections.Generic;

namespace StaticMlp.Networking.Replication {
    public sealed class EntitySnapshotBatch {
        public NetworkPeerId SourcePeer;
        public readonly List<byte[]> Payloads = new();
    }
}
