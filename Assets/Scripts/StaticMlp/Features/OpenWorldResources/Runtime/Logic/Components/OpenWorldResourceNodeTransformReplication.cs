using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldResources
{
    public static class OpenWorldResourceNodeTransformReplication
    {
        public const ushort TYPE_ID = 55422;
        public const ReplicationAuthority AUTHORITY = ReplicationAuthority.Server;
        public const ReplicationAudience AUDIENCE = ReplicationAudience.All;
        public const NetDelivery DELIVERY = NetDelivery.ReliableSequenced;

        public static ComponentDelta CreateDelta(EntityGID gid, in OpenWorldResourceNodeTransform state)
        {
            var writer = BinaryPackWriter.CreateFromPool(20);
            writer.WriteFloat(state.Position.x, state.Position.y, state.Position.z);
            writer.WriteFloat(state.YawDegrees);
            writer.WriteFloat(state.Scale);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return new ComponentDelta(gid, TYPE_ID, bytes);
        }

        public static OpenWorldResourceNodeTransform Read(byte[] payload)
        {
            if (payload == null || payload.Length == 0)
                return default;

            var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
            return new OpenWorldResourceNodeTransform
            {
                Position = new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()),
                YawDegrees = reader.ReadFloat(),
                Scale = reader.ReadFloat()
            };
        }
    }
}
