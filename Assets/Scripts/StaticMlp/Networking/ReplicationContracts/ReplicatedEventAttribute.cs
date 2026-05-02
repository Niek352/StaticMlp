using System;

namespace StaticMlp.Networking.Replication {
    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class ReplicatedEventAttribute : Attribute {
        public readonly NetDelivery Delivery;

        public ReplicatedEventAttribute(NetDelivery delivery) {
            Delivery = delivery;
        }
    }
}
