using FFS.Libraries.StaticEcs;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public sealed class OpenWorldTerrainStreamingConfig : IResource
    {
        public WorldGenerationSeed Seed = new(OpenWorldGenerationConfig.DEFAULT_WORLD_SEED);
        public WorldChunkBounds Bounds = WorldChunkBounds.Default;
        public float ChunkWorldSize = OpenWorldGenerationConfig.DEFAULT_CHUNK_WORLD_SIZE;
        public int BaseQuadCount = OpenWorldGenerationConfig.DEFAULT_BASE_QUAD_COUNT;
        public int ViewRadiusInChunks = OpenWorldGenerationConfig.DEFAULT_VIEW_RADIUS_IN_CHUNKS;
        public int ColliderRadiusInChunks = OpenWorldGenerationConfig.DEFAULT_COLLIDER_RADIUS_IN_CHUNKS;
        public int MaxChunkLoadsPerFrame = OpenWorldGenerationConfig.DEFAULT_MAX_CLIENT_CHUNK_LOADS_PER_FRAME;
        public bool AddSkirts = OpenWorldGenerationConfig.DEFAULT_ADD_SKIRTS;
        public float SkirtDepth = OpenWorldGenerationConfig.DEFAULT_SKIRT_DEPTH;
        public string RootName = "Open World Terrain";
        public Color MaterialColor = OpenWorldGenerationConfig.DefaultTerrainMaterialColor;
        public bool ShowDebugGizmos = true;
        public bool ShowDebugGizmosInEditMode;
        public bool ShowDebugChunkLabels = true;
        public bool LogDebugStreaming = true;
        public float DebugLogIntervalSeconds = 1f;
        public float DebugGizmoHeight = OpenWorldGenerationConfig.DEFAULT_DEBUG_GIZMO_HEIGHT;
        public Color DebugWorldBoundsColor = OpenWorldGenerationConfig.DebugWorldBoundsColor;
        public Color DebugColliderChunkColor = OpenWorldGenerationConfig.DebugColliderChunkColor;
        public Color DebugLod0ChunkColor = OpenWorldGenerationConfig.DebugLod0ChunkColor;
        public Color DebugLod1ChunkColor = OpenWorldGenerationConfig.DebugLod1ChunkColor;
        public Color DebugLod2ChunkColor = OpenWorldGenerationConfig.DebugLod2ChunkColor;
        public Color DebugLod3ChunkColor = OpenWorldGenerationConfig.DebugLod3ChunkColor;

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
