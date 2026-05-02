using System.Collections.Generic;

namespace StaticMlp.Networking.Transport {
    public static class ServerPeerRegistry {
        public static readonly List<NetworkPeerId> Peers = new();

        public static void Add(NetworkPeerId peer) {
            if (!Peers.Contains(peer))
                Peers.Add(peer);
        }

        public static void Remove(NetworkPeerId peer) {
            Peers.Remove(peer);
        }

        public static void Clear() => Peers.Clear();
    }
}
