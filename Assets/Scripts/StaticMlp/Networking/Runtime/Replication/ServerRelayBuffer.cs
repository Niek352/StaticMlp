using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public sealed class ServerRelayBuffer : IResource {
        public readonly List<ServerRelayItem> Items = new();

        public void Add(NetworkPeerId sourcePeer, byte[] payload, NetDelivery delivery) {
            Items.Add(new ServerRelayItem(sourcePeer, payload, delivery));
        }

        public void Clear() => Items.Clear();
    }
}
