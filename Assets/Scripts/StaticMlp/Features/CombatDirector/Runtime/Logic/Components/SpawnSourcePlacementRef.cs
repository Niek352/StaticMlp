using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration;

namespace StaticMlp.Features.CombatDirector
{
    public struct SpawnSourcePlacementRef : IComponent
    {
        public WorldChunkId ChunkId;
        public int PlacementIndex;
        public SpawnPlacementKindId KindId;
    }
}
