using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Progression
{
    public static class Stage1ProgressionStateReplication
    {
        public const ushort TYPE_ID = 58031;
        public const ReplicationAuthority AUTHORITY = ReplicationAuthority.Server;
        public const ReplicationAudience AUDIENCE = ReplicationAudience.All;
        public const NetDelivery DELIVERY = NetDelivery.ReliableSequenced;

        public static ComponentDelta CreateDelta(EntityGID gid, in Stage1ProgressionState state)
        {
            var writer = BinaryPackWriter.CreateFromPool(16);
            writer.WriteUshort(state.AnchorId);
            writer.WriteUint(state.AppliedFlagsMask);
            writer.WriteUint(state.AppliedRewardsMask);
            writer.WriteByte(state.BossPreparationTokens);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            
            return new ComponentDelta(gid, TYPE_ID, bytes);
        }

        public static Stage1ProgressionState Read(byte[] payload)
        {
            var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
            return new Stage1ProgressionState
            {
                AnchorId = reader.ReadUshort(),
                AppliedFlagsMask = reader.ReadUint(),
                AppliedRewardsMask = reader.ReadUint(),
                BossPreparationTokens = reader.ReadByte()
            };
        }
    }
}
