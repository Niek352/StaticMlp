using System;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class SimpleWorldGenerationService : IWorldGenerationService
    {
        public GeneratedChunkData GenerateChunk(WorldChunkId chunkId, WorldGenerationRequest request)
        {
            if (!request.Bounds.Contains(chunkId))
                throw new ArgumentOutOfRangeException(nameof(chunkId), chunkId, "Requested chunk is outside finite world bounds.");

            var sampler = new SimpleSurfaceSampler(request.Seed);
            var mesh = TerrainMeshBuilder.Build(
                new TerrainMeshBuildRequest(
                    chunkId,
                    request.ChunkWorldSize,
                    request.Lod,
                    request.BaseQuadCount,
                    request.AddSkirts,
                    request.SkirtDepth),
                sampler);

            return new GeneratedChunkData(chunkId, request.Lod, mesh);
        }
    }
}
