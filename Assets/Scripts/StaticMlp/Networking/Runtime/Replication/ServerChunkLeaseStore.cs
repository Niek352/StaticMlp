using System;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Networking.Replication
{
    public sealed class ServerChunkLeaseStore : IResource
    {
        public const int DEFAULT_CHUNKS_PER_PEER = 4;

        private readonly Dictionary<NetworkPeerId, uint[]> _leases = new();
        private readonly int _chunksPerPeer;

        public ServerChunkLeaseStore(int chunksPerPeer = DEFAULT_CHUNKS_PER_PEER)
        {
            if (chunksPerPeer <= 0)
                throw new ArgumentOutOfRangeException(nameof(chunksPerPeer), chunksPerPeer, "Chunk lease size must be positive.");

            _chunksPerPeer = chunksPerPeer;
        }

        public uint[] GetOrCreate(NetworkPeerId peer)
        {
            if (peer.Value == 0)
                throw new ArgumentOutOfRangeException(nameof(peer), peer, "Server peer cannot receive a client chunk lease.");

            if (_leases.TryGetValue(peer, out var existing))
                return existing;

            var created = new uint[_chunksPerPeer];
            for (var i = 0; i < created.Length; i++)
            {
                var chunkInfo = SW.FindNextSelfFreeChunk();
                SW.RegisterChunk(chunkInfo.ChunkIdx, ChunkOwnerType.Other, clusterId: 0);
                created[i] = chunkInfo.ChunkIdx;
            }

            _leases.Add(peer, created);
            return created;
        }
    }
}
