using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class OpenWorldChunkOverlayDirtyQueue : IResource
    {
        private readonly HashSet<WorldChunkId> _dirtyChunks = new();

        public void MarkDirty(WorldChunkId chunkId)
        {
            _dirtyChunks.Add(chunkId);
        }

        public void Drain(List<WorldChunkId> output)
        {
            foreach (var chunkId in _dirtyChunks)
                output.Add(chunkId);

            _dirtyChunks.Clear();
        }
    }
}
