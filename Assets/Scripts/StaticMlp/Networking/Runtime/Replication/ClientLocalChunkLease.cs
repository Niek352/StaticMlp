using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication
{
    public sealed class ClientLocalChunkLease : IResource
    {
        private uint[] _chunkIds = Array.Empty<uint>();

        public int Count => _chunkIds.Length;

        public uint GetChunkId(int index) => _chunkIds[index];

        public void Replace(uint[] chunkIds)
        {
            _chunkIds = chunkIds ?? Array.Empty<uint>();
        }
    }
}
