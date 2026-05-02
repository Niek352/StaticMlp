using System;
using StaticMlp.Networking;

namespace StaticMlp.Networking.Replication {
    public enum ReplicationAuthority : byte {
        Server = 0,
        Owner = 1
    }

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

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class ReplicatedFieldAttribute : Attribute {
        public float Quantize;
        public bool Compress;
    }

    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class ReplicatedEventAttribute : Attribute {
        public readonly NetDelivery Delivery;

        public ReplicatedEventAttribute(NetDelivery delivery) {
            Delivery = delivery;
        }
    }
}
