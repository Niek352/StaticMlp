namespace StaticMlp.Features.OpenWorldGeneration
{
    public readonly struct TerrainMeshBuildRequest
    {
        public readonly WorldChunkId ChunkId;
        public readonly float ChunkWorldSize;
        public readonly int Lod;
        public readonly int BaseQuadCount;
        public readonly bool AddSkirts;
        public readonly float SkirtDepth;

        public TerrainMeshBuildRequest(
            WorldChunkId chunkId,
            float chunkWorldSize,
            int lod,
            int baseQuadCount,
            bool addSkirts,
            float skirtDepth)
        {
            ChunkId = chunkId;
            ChunkWorldSize = chunkWorldSize;
            Lod = lod;
            BaseQuadCount = baseQuadCount;
            AddSkirts = addSkirts;
            SkirtDepth = skirtDepth;
        }
    }
}
