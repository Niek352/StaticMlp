using System;

namespace StaticMlp.Networking.Replication {
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class ReplicatedComponentAttribute : Attribute {
        public readonly ReplicationAuthority Authority;
        public readonly ReplicationAudience Audience;
        public readonly NetDelivery Delivery;
        public readonly ushort SendRate;
        public readonly string Guid;

        public ReplicatedComponentAttribute(
            ReplicationAuthority authority,
            NetDelivery delivery,
            ushort sendRate = 20,
            ReplicationAudience audience = ReplicationAudience.All,
            string guid = null) {
            Authority = authority;
            Audience = audience;
            Delivery = delivery;
            SendRate = sendRate;
            Guid = guid;
        }
    }
}
