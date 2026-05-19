using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldTerrainStreamingConfig : IResource
    {
        public WorldGenerationSeed Seed = new(12345);
        public WorldChunkBounds Bounds = WorldChunkBounds.Default;
        public float ChunkWorldSize = 128f;
        public int BaseQuadCount = 12;
        public int ViewRadiusInChunks = 4;
        public int ColliderRadiusInChunks = 1;
        public int MaxChunkLoadsPerFrame = 4;
        public bool AddSkirts = true;
        public float SkirtDepth = 6f;
        public string RootName = "Open World Terrain";
        public Color MaterialColor = new(0.52f, 0.62f, 0.42f, 1f);
        public bool ShowDebugGizmos = true;
        public bool ShowDebugGizmosInEditMode;
        public bool ShowDebugChunkLabels = true;
        public bool LogDebugStreaming = true;
        public float DebugLogIntervalSeconds = 1f;
        public float DebugGizmoHeight = 24f;
        public Color DebugWorldBoundsColor = new(1f, 0.82f, 0.18f, 1f);
        public Color DebugColliderChunkColor = new(1f, 0.28f, 0.18f, 1f);
        public Color DebugLod0ChunkColor = new(0.1f, 0.9f, 0.35f, 1f);
        public Color DebugLod1ChunkColor = new(0.18f, 0.65f, 1f, 1f);
        public Color DebugLod2ChunkColor = new(0.72f, 0.48f, 1f, 1f);
        public Color DebugLod3ChunkColor = new(0.75f, 0.75f, 0.75f, 1f);

        public static OpenWorldTerrainStreamingConfig Default()
        {
            return new OpenWorldTerrainStreamingConfig();
        }

        public WorldGenerationRequest CreateGenerationRequest(int lod)
        {
            return new WorldGenerationRequest(
                Seed,
                Bounds,
                ChunkWorldSize,
                BaseQuadCount,
                lod,
                AddSkirts,
                SkirtDepth);
        }
    }
}
