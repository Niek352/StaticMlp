using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.OpenWorldGeneration
{
    /// <summary>
    /// Result of chunk generation. Published after job pipeline completes.
    /// </summary>
    public readonly struct OpenWorldChunkGenerationCompleted : IEvent
    {
        public readonly WorldChunkId ChunkId;
        public readonly int Lod;
        public readonly GenerationOutputMask Outputs;
        public readonly TerrainMeshData TerrainMesh;
        public readonly TerrainMeshData PhysicsMesh;
        public readonly TerrainMeshData NavMeshSourceMesh;
        public readonly ResourcePlacement[] ResourcePlacements;
        public readonly SpawnPlacement[] SpawnPlacements;

        public OpenWorldChunkGenerationCompleted(
            WorldChunkId chunkId,
            int lod,
            GenerationOutputMask outputs,
            TerrainMeshData terrainMesh,
            TerrainMeshData physicsMesh,
            TerrainMeshData navMeshSourceMesh,
            ResourcePlacement[] resourcePlacements,
            SpawnPlacement[] spawnPlacements)
        {
            ChunkId = chunkId;
            Lod = lod;
            Outputs = outputs;
            TerrainMesh = terrainMesh;
            PhysicsMesh = physicsMesh;
            NavMeshSourceMesh = navMeshSourceMesh;
            ResourcePlacements = resourcePlacements;
            SpawnPlacements = spawnPlacements;
        }
    }
}
