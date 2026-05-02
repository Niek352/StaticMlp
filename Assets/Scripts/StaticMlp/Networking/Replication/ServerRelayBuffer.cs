using System.Collections.Generic;

namespace StaticMlp.Networking.Replication {
    public readonly struct ServerRelayItem {
        public readonly NetworkPeerId SourcePeer;
        public readonly ComponentDelta Delta;

        public ServerRelayItem(NetworkPeerId sourcePeer, ComponentDelta delta) {
            SourcePeer = sourcePeer;
            Delta = delta;
        }
    }

    public static class ServerRelayBuffer {
        public static readonly List<ServerRelayItem> Items = new();

        public static void Add(NetworkPeerId sourcePeer, ComponentDelta delta) {
            Items.Add(new ServerRelayItem(sourcePeer, delta));
        }

        public static void Clear() => Items.Clear();
    }
}
