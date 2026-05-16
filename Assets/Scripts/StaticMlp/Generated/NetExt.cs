using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Generated
{
    public static class NetExt
    {
        public static void WriteWorldChunkId(this ref NetworkWriter writer, WorldChunkId worldChunkId)
        {
            writer.WriteInt(worldChunkId.X);
            writer.WriteInt(worldChunkId.Z);
        }
        
        
        public static WorldChunkId ReadWorldChunkId(this ref NetworkReader writer)
        {
            return new WorldChunkId(writer.ReadInt(), writer.ReadInt());
        }
    }
}