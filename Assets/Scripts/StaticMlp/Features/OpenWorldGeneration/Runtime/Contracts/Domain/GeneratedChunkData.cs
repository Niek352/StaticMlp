using System;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class GeneratedChunkData
    {
        public GeneratedChunkData(WorldChunkId chunkId, int lod, TerrainMeshData terrainMesh)
            : this(chunkId, lod, terrainMesh, Array.Empty<ResourcePlacement>(), Array.Empty<SpawnPlacement>())
        {
        }

        public GeneratedChunkData(
            WorldChunkId chunkId,
            int lod,
            TerrainMeshData terrainMesh,
            ResourcePlacement[] resourcePlacements,
            SpawnPlacement[] spawnPlacements)
        {
            ChunkId = chunkId;
            Lod = lod;
            TerrainMesh = terrainMesh;
            ResourcePlacements = resourcePlacements ?? throw new ArgumentNullException(nameof(resourcePlacements));
            SpawnPlacements = spawnPlacements ?? throw new ArgumentNullException(nameof(spawnPlacements));
        }

        public readonly WorldChunkId ChunkId;
        public readonly int Lod;
        public readonly TerrainMeshData TerrainMesh;
        public readonly ResourcePlacement[] ResourcePlacements;
        public readonly SpawnPlacement[] SpawnPlacements;
    }
}
