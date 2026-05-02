using System.Collections.Generic;

namespace StaticMlp.Networking.Replication {
    public sealed class ComponentBatch {
        public NetworkPeerId SourcePeer;
        public readonly List<ComponentDelta> Deltas = new();
    }
}
