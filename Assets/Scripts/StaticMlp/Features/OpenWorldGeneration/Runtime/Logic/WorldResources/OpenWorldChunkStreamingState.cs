using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldChunkStreamingState : IResource
    {
        private readonly Dictionary<NetworkPeerId, HashSet<WorldChunkId>> _loadedChunksByPeer = new();
        private readonly Dictionary<WorldChunkId, byte[]> _snapshotsByChunk = new();
        private readonly HashSet<WorldChunkId> _serverLoadedChunks = new();

        public readonly List<PendingSnapshot> PendingSnapshots = new();

        public HashSet<WorldChunkId> LoadedChunksFor(NetworkPeerId peer)
        {
            if (_loadedChunksByPeer.TryGetValue(peer, out var chunks))
                return chunks;

            chunks = new HashSet<WorldChunkId>();
            _loadedChunksByPeer.Add(peer, chunks);
            return chunks;
        }

        public void CopyTrackedPeers(List<NetworkPeerId> peers)
        {
            foreach (var peer in _loadedChunksByPeer.Keys)
                peers.Add(peer);
        }

        public void RemovePeer(NetworkPeerId peer)
        {
            _loadedChunksByPeer.Remove(peer);
        }

        public bool AnyPeerHasChunk(WorldChunkId chunkId)
        {
            foreach (var pair in _loadedChunksByPeer)
                if (pair.Value.Contains(chunkId))
                    return true;

            return false;
        }

        public bool ServerHasLoadedChunk(WorldChunkId chunkId)
        {
            return _serverLoadedChunks.Contains(chunkId);
        }

        public void MarkServerLoaded(WorldChunkId chunkId)
        {
            _serverLoadedChunks.Add(chunkId);
        }

        public void MarkServerUnloaded(WorldChunkId chunkId)
        {
            _serverLoadedChunks.Remove(chunkId);
        }

        public bool TryGetSnapshot(WorldChunkId chunkId, out byte[] snapshot)
        {
            return _snapshotsByChunk.TryGetValue(chunkId, out snapshot);
        }

        public void SetSnapshot(WorldChunkId chunkId, byte[] snapshot)
        {
            _snapshotsByChunk[chunkId] = snapshot;
        }

        public void QueueSnapshot(NetworkPeerId peer, WorldChunkId chunkId, ushort clusterId)
        {
            PendingSnapshots.Add(new PendingSnapshot(peer, chunkId, clusterId));
        }

        public readonly struct PendingSnapshot
        {
            public PendingSnapshot(NetworkPeerId peer, WorldChunkId chunkId, ushort clusterId)
            {
                Peer = peer;
                ChunkId = chunkId;
                ClusterId = clusterId;
            }

            public readonly NetworkPeerId Peer;
            public readonly WorldChunkId ChunkId;
            public readonly ushort ClusterId;
        }
    }
}
