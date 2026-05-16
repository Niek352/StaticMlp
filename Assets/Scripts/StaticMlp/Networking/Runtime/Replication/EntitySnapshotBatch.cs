using System.Collections.Generic;

namespace StaticMlp.Networking.Replication {
    public struct EntitySnapshotBatch {
        public NetworkPeerId SourcePeer;
        public List<byte[]> Payloads;
    }
}
