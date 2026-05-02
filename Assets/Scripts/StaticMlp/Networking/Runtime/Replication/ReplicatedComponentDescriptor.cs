namespace StaticMlp.Networking.Replication {
    public readonly struct ReplicatedComponentDescriptor {
        public readonly ushort TypeId;
        public readonly ReplicationAuthority Authority;
        public readonly NetDelivery Delivery;
        public readonly ushort SendRate;
        public readonly byte LayoutVersion;

        public ReplicatedComponentDescriptor(
            ushort typeId,
            ReplicationAuthority authority,
            NetDelivery delivery,
            ushort sendRate,
            byte layoutVersion
        ) {
            TypeId = typeId;
            Authority = authority;
            Delivery = delivery;
            SendRate = sendRate;
            LayoutVersion = layoutVersion;
        }
    }
}
