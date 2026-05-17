using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;

namespace StaticMlp.Features.OpenWorldResources
{
    public sealed class OpenWorldPlacementIndexStore : IResource
    {
        private readonly Dictionary<WorldChunkId, ResourcePlacement[]> _resourcesByChunk = new();
        private readonly Dictionary<long, ResourcePlacement> _resourceByPlacementId = new();

        public void RegisterChunkPlacements(WorldChunkId chunkId, ResourcePlacement[] placements)
        {
            if (_resourcesByChunk.TryGetValue(chunkId, out var previous))
            {
                for (var i = 0; i < previous.Length; i++)
                    _resourceByPlacementId.Remove(previous[i].PlacementId);
            }

            _resourcesByChunk[chunkId] = placements;
            for (var i = 0; i < placements.Length; i++)
                _resourceByPlacementId[placements[i].PlacementId] = placements[i];
        }

        public bool TryGetPlacement(long placementId, out ResourcePlacement placement)
        {
            return _resourceByPlacementId.TryGetValue(placementId, out placement);
        }

        public bool TryGetChunkPlacements(WorldChunkId chunkId, out ResourcePlacement[] placements)
        {
            return _resourcesByChunk.TryGetValue(chunkId, out placements);
        }

        public int PlacementCount => _resourceByPlacementId.Count;
    }
}
