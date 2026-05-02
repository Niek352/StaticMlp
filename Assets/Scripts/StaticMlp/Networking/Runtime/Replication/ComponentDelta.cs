using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication {
    public readonly struct ComponentDelta {
        public readonly EntityGID Gid;
        public readonly ushort ComponentTypeId;
        public readonly byte[] Payload;

        public ComponentDelta(EntityGID gid, ushort componentTypeId, byte[] payload) {
            Gid = gid;
            ComponentTypeId = componentTypeId;
            Payload = payload;
        }
    }
}
