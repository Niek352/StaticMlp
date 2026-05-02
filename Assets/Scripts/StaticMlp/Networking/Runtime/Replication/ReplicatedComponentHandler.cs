using System;

namespace StaticMlp.Networking.Replication {
    public readonly struct ReplicatedComponentHandler {
        public readonly ReplicatedComponentDescriptor Descriptor;
        public readonly Action<CW.Entity, ComponentDelta> ApplyClient;
        public readonly Action<SW.Entity, ComponentDelta> ApplyServer;
        public readonly Action<CW.Entity, NetOutbox, NetworkPeerId> CollectClient;
        public readonly Action<SW.Entity, NetOutbox, NetworkPeerId> CollectServer;

        public ReplicatedComponentHandler(
            ReplicatedComponentDescriptor descriptor,
            Action<CW.Entity, ComponentDelta> applyClient,
            Action<SW.Entity, ComponentDelta> applyServer,
            Action<CW.Entity, NetOutbox, NetworkPeerId> collectClient,
            Action<SW.Entity, NetOutbox, NetworkPeerId> collectServer
        ) {
            Descriptor = descriptor;
            ApplyClient = applyClient;
            ApplyServer = applyServer;
            CollectClient = collectClient;
            CollectServer = collectServer;
        }
    }
}
