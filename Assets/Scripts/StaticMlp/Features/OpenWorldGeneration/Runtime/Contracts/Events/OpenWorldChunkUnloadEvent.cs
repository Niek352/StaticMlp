using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.OpenWorldGeneration
{
    [ReplicatedEvent(NetDelivery.ReliableSequenced)]
    public readonly struct OpenWorldChunkUnloadEvent : IEvent
    {
        public const ushort NETWORK_EVENT_ID = 59031;

        public readonly WorldChunkId ChunkId;
        public readonly ushort ClusterId;

        public OpenWorldChunkUnloadEvent(WorldChunkId chunkId, ushort clusterId)
        {
            ChunkId = chunkId;
            ClusterId = clusterId;
        }

        public static byte[] Write(in OpenWorldChunkUnloadEvent evt)
        {
            var writer = BinaryPackWriter.CreateFromPool(10);
            writer.WriteInt(evt.ChunkId.X);
            writer.WriteInt(evt.ChunkId.Z);
            writer.WriteUshort(evt.ClusterId);
            var bytes = writer.CopyToBytes();
            writer.Dispose();
            return bytes;
        }

        public static bool TryRead(byte[] payload, out OpenWorldChunkUnloadEvent evt)
        {
            try
            {
                if (payload == null || payload.Length == 0)
                {
                    evt = default;
                    return false;
                }

                var reader = new BinaryPackReader(payload, (uint)payload.Length, 0);
                evt = new OpenWorldChunkUnloadEvent(
                    new WorldChunkId(reader.ReadInt(), reader.ReadInt()),
                    reader.ReadUshort());
                return true;
            }
            catch
            {
                evt = default;
                return false;
            }
        }
    }
}
