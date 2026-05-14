namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class GeneratedChunkData
    {
        public GeneratedChunkData(WorldChunkId chunkId, int lod, TerrainMeshData terrainMesh)
        {
            ChunkId = chunkId;
            Lod = lod;
            TerrainMesh = terrainMesh;
        }

        public readonly WorldChunkId ChunkId;
        public readonly int Lod;
        public readonly TerrainMeshData TerrainMesh;
    }
}
