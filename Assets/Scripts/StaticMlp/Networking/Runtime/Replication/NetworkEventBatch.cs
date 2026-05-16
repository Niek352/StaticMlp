using System.Collections.Generic;

namespace StaticMlp.Networking.Replication {
    public sealed class NetworkEventBatch {
        internal readonly List<NetworkEventPacket> Events = new();
    }
}
