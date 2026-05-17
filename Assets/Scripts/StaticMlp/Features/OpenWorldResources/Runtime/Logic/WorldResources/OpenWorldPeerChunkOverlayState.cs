using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Networking;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class OpenWorldPeerChunkOverlayState : IResource
    {
        private readonly Dictionary<NetworkPeerId, Dictionary<WorldChunkId, uint>> _acked = new();

        public uint GetAckedRevision(NetworkPeerId peer, WorldChunkId chunkId)
        {
            return _acked.TryGetValue(peer, out var chunks) && chunks.TryGetValue(chunkId, out var revision)
                ? revision
                : 0;
        }

        public void SetAckedRevision(NetworkPeerId peer, WorldChunkId chunkId, uint revision)
        {
            if (!_acked.TryGetValue(peer, out var chunks))
            {
                chunks = new Dictionary<WorldChunkId, uint>();
                _acked.Add(peer, chunks);
            }

            chunks[chunkId] = revision;
        }

        public void RemovePeer(NetworkPeerId peer)
        {
            _acked.Remove(peer);
        }
    }
}
