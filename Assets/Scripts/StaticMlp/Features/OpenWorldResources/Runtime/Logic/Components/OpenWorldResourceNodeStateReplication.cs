using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.OpenWorldResources
{
    public static class OpenWorldResourceNodeStateReplication
    {
        public const ushort TYPE_ID = 28580;
        public const ReplicationAuthority AUTHORITY = ReplicationAuthority.Server;
        public const ReplicationAudience AUDIENCE = ReplicationAudience.All;
        public const NetDelivery DELIVERY = NetDelivery.ReliableSequenced;

        public static ComponentDelta CreateDelta(EntityGID gid, in OpenWorldResourceNodeState state)
        {
            var writer = BinaryPackWriter.CreateFromPool(16);
            writer.WriteLong(state.PlacementId);
            writer.WriteUshort(state.KindIdValue);
            writer.WriteInt(state.RemainingAmount);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return new ComponentDelta(gid, TYPE_ID, bytes);
        }

        public static OpenWorldResourceNodeState Read(byte[] payload)
        {
            if (payload == null || payload.Length == 0)
                return default;

            var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
            return new OpenWorldResourceNodeState
            {
                PlacementId = reader.ReadLong(),
                KindIdValue = reader.ReadUshort(),
                RemainingAmount = reader.ReadInt()
            };
        }
    }
}
