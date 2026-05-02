using System;

namespace StaticMlp.Networking.Replication {
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class ReplicatedComponentAttribute : Attribute {
        public readonly ReplicationAuthority Authority;
        public readonly NetDelivery Delivery;
        public readonly ushort SendRate;

        public ReplicatedComponentAttribute(ReplicationAuthority authority, NetDelivery delivery, ushort sendRate = 20) {
            Authority = authority;
            Delivery = delivery;
            SendRate = sendRate;
        }
    }
}
