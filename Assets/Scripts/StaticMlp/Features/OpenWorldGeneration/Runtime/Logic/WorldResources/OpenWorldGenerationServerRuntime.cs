using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldGenerationServerRuntime : IResource, IDisposable
    {
        public const int DEFAULT_MAX_CHUNK_GENERATIONS_PER_FRAME = 1;
        public const int DEFAULT_MAX_CLUSTER_SNAPSHOTS_PER_FRAME = 1;
        public const int DEFAULT_STATIC_STREAMING_RADIUS_IN_CHUNKS = 4;
        public const int DEFAULT_SERVER_GEOMETRY_LOD = 1;

        public OpenWorldGenerationServerRuntime(
            WorldGenerationRequest defaultRequest,
            int maxChunkGenerationsPerFrame = DEFAULT_MAX_CHUNK_GENERATIONS_PER_FRAME,
            int staticStreamingRadiusInChunks = DEFAULT_STATIC_STREAMING_RADIUS_IN_CHUNKS,
            int serverGeometryLod = DEFAULT_SERVER_GEOMETRY_LOD,
            int maxClusterSnapshotsPerFrame = DEFAULT_MAX_CLUSTER_SNAPSHOTS_PER_FRAME)
        {
            DefaultRequest = defaultRequest;
            if (maxChunkGenerationsPerFrame <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxChunkGenerationsPerFrame), maxChunkGenerationsPerFrame, "Chunk generation budget must be positive.");
            if (maxClusterSnapshotsPerFrame <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxClusterSnapshotsPerFrame), maxClusterSnapshotsPerFrame, "Cluster snapshot budget must be positive.");
            if (staticStreamingRadiusInChunks < 0)
                throw new ArgumentOutOfRangeException(nameof(staticStreamingRadiusInChunks), staticStreamingRadiusInChunks, "Static streaming radius must be non-negative.");
            if (serverGeometryLod < 0)
                throw new ArgumentOutOfRangeException(nameof(serverGeometryLod), serverGeometryLod, "Server geometry LOD must be non-negative.");

            MaxChunkGenerationsPerFrame = maxChunkGenerationsPerFrame;
            MaxClusterSnapshotsPerFrame = maxClusterSnapshotsPerFrame;
            StaticStreamingRadiusInChunks = staticStreamingRadiusInChunks;
            ServerGeometryLod = serverGeometryLod;
        }

        public readonly WorldGenerationRequest DefaultRequest;
        public readonly int MaxChunkGenerationsPerFrame;
        public readonly int MaxClusterSnapshotsPerFrame;
        public readonly int StaticStreamingRadiusInChunks;
        public readonly int ServerGeometryLod;

        public static OpenWorldGenerationServerRuntime CreateDefault()
        {
            return new OpenWorldGenerationServerRuntime(
                new WorldGenerationRequest(
                    new WorldGenerationSeed(12345),
                    WorldChunkBounds.Default,
                    128f,
                    64,
                    0,
                    true,
                    6f),
                DEFAULT_MAX_CHUNK_GENERATIONS_PER_FRAME);
        }

        public WorldGenerationRequest CreateServerGeometryRequest()
        {
            return new WorldGenerationRequest(
                DefaultRequest.Seed,
                DefaultRequest.Bounds,
                DefaultRequest.ChunkWorldSize,
                DefaultRequest.BaseQuadCount,
                ServerGeometryLod,
                false,
                DefaultRequest.SkirtDepth);
        }

        public void Dispose()
        {
        }
    }
}
