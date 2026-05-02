using System.Collections.Generic;

namespace StaticMlp.Networking.Transport {
    public static class ServerDisconnectedPeerQueue {
        public static readonly Queue<NetworkPeerId> Peers = new();

        public static void Enqueue(NetworkPeerId peer) {
            Peers.Enqueue(peer);
        }

        public static bool TryDequeue(out NetworkPeerId peer) {
            if (Peers.Count == 0) {
                peer = default;
                return false;
            }

            peer = Peers.Dequeue();
            return true;
        }

        public static void Clear() => Peers.Clear();
    }
}
