using System;

namespace StaticMlp.Networking.Replication
{
    public readonly struct ChunkLeaseMessage
    {
        public readonly uint[] ChunkIds;

        public ChunkLeaseMessage(uint[] chunkIds)
        {
            ChunkIds = chunkIds ?? Array.Empty<uint>();
        }
    }
}
