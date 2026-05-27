using UnityEngine;

namespace StaticMlp.Features.OpenWorldGeneration
{
    public static class OpenWorldGenerationConfig
    {
        public const int DEFAULT_WORLD_SEED = 12345;
        public const float DEFAULT_CHUNK_WORLD_SIZE = 128f;
        public const int DEFAULT_BASE_QUAD_COUNT = 12;
        public const int DEFAULT_LOD = 0;
        public const bool DEFAULT_ADD_SKIRTS = true;
        public const float DEFAULT_SKIRT_DEPTH = 6f;
        public const int DEFAULT_VIEW_RADIUS_IN_CHUNKS = 4;
        public const int DEFAULT_COLLIDER_RADIUS_IN_CHUNKS = 1;
        public const int DEFAULT_MAX_CLIENT_CHUNK_LOADS_PER_FRAME = 4;
        public const float DEFAULT_DEBUG_GIZMO_HEIGHT = 24f;
        public const int DEFAULT_MAX_CHUNK_GENERATIONS_PER_FRAME = 1;
        public const int DEFAULT_MAX_CLUSTER_SNAPSHOTS_PER_FRAME = 1;
        public const int DEFAULT_STATIC_STREAMING_RADIUS_IN_CHUNKS = 4;
        public const int DEFAULT_SERVER_GEOMETRY_LOD = 1;

        public const int HEIGHTMAP_RESOLUTION = 128;
        public const int SURFACE_HEIGHT_PADDING_SAMPLES = 1;

        public const float WATER_LEVEL = -7f;
        public const float WATER_BIOME_MASK_THRESHOLD = 0.5f;
        public const float MOUNTAIN_BIOME_MIN_HEIGHT = 14f;
        public const float WET_BIOME_MIN_MOISTURE = 0.55f;
        public const float ROCK_MATERIAL_MIN_HEIGHT = 8f;
        public const float HIGH_ROCK_MATERIAL_MIN_HEIGHT = 16f;
        public const float WETNESS_HEIGHT_OFFSET = 3f;
        public const float WETNESS_HEIGHT_RANGE = 6f;
        public const float WETNESS_MOISTURE_WEIGHT = 0.35f;
        public const float SURFACE_NORMAL_SAMPLE_STEP = 1f;

        public const float SURFACE_BASE_NOISE_SCALE = 0.0065f;
        public const float SURFACE_BASE_NOISE_BIAS = 0.48f;
        public const float SURFACE_BASE_NOISE_AMPLITUDE = 42f;
        public const float SURFACE_DETAIL_NOISE_SCALE = 0.021f;
        public const float SURFACE_DETAIL_NOISE_BIAS = 0.5f;
        public const float SURFACE_DETAIL_NOISE_AMPLITUDE = 9f;
        public const float SURFACE_RIDGE_SCALE_X = 0.018f;
        public const float SURFACE_RIDGE_SCALE_Z = 0.015f;
        public const float SURFACE_RIDGE_AMPLITUDE = 4f;
        public const float SURFACE_MOISTURE_NOISE_SCALE = 0.004f;
        public const int SURFACE_SEED_OFFSET_X_SALT = 92821;
        public const int SURFACE_SEED_OFFSET_Z_SALT = 51787;
        public const float SURFACE_SEED_OFFSET_SCALE = 0.37f;
        public const float NATIVE_HEIGHT_LOW_SEED_X_SCALE = 17.13f;
        public const float NATIVE_HEIGHT_LOW_SEED_Z_SCALE = 9.71f;
        public const float NATIVE_HEIGHT_LOW_NOISE_SCALE = 0.0065f;
        public const float NATIVE_HEIGHT_LOW_AMPLITUDE = 24f;
        public const float NATIVE_HEIGHT_MID_SEED_X_SCALE = 3.37f;
        public const float NATIVE_HEIGHT_MID_SEED_Z_SCALE = 2.11f;
        public const float NATIVE_HEIGHT_MID_NOISE_SCALE_X = 0.021f;
        public const float NATIVE_HEIGHT_MID_NOISE_SCALE_Z = 0.008f;
        public const float NATIVE_HEIGHT_MID_AMPLITUDE = 7f;
        public const float NATIVE_HEIGHT_RIDGE_NOISE_SCALE_X = 0.018f;
        public const float NATIVE_HEIGHT_RIDGE_NOISE_SCALE_Z = 0.015f;
        public const float NATIVE_HEIGHT_RIDGE_AMPLITUDE = 8f;
        public const float NATIVE_HEIGHT_OFFSET = -9f;
        public const byte WATER_VERTEX_COLOR_R = 50;
        public const byte WATER_VERTEX_COLOR_G = 95;
        public const byte WATER_VERTEX_COLOR_B = 150;
        public const byte GRASS_VERTEX_COLOR_R = 72;
        public const byte GRASS_VERTEX_COLOR_G = 122;
        public const byte GRASS_VERTEX_COLOR_B = 62;
        public const byte DIRT_VERTEX_COLOR_R = 105;
        public const byte DIRT_VERTEX_COLOR_G = 94;
        public const byte DIRT_VERTEX_COLOR_B = 74;
        public const byte ROCK_VERTEX_COLOR_R = 170;
        public const byte ROCK_VERTEX_COLOR_G = 164;
        public const byte ROCK_VERTEX_COLOR_B = 140;
        public const byte FALLBACK_VERTEX_COLOR_R = 82;
        public const byte FALLBACK_VERTEX_COLOR_G = 110;
        public const byte FALLBACK_VERTEX_COLOR_B = 72;
        public const byte VERTEX_COLOR_A = 255;

        public const int RESOURCE_CANDIDATES_PER_CHUNK = 24;
        public const int SPAWN_CANDIDATES_PER_CHUNK = 8;
        public const int PLACEMENT_FALLBACK_GRID_SIZE = 16;
        public const float PLACEMENT_MAX_WATER_MASK = 0.35f;
        public const float PLACEMENT_MIN_NORMAL_Y = 0.85f;
        public const float RESOURCE_MARGIN_FRACTION = 0.08f;
        public const float SPAWN_MARGIN_FRACTION = 0.18f;
        public const float RESOURCE_SCALE_MIN = 0.8f;
        public const float RESOURCE_SCALE_MAX = 1.35f;
        public const float FALLBACK_RESOURCE_SCALE_MIN = 0.9f;
        public const float FALLBACK_RESOURCE_SCALE_MAX = 1.15f;
        public const float SPAWN_SCALE_MIN = 0.9f;
        public const float SPAWN_SCALE_MAX = 1.15f;
        public const float CHEST_PLACEMENT_CHANCE = 0.08f;
        public const float SPORE_MIN_WETNESS = 0.42f;

        public const ushort TREE_RESOURCE_KIND = 1;
        public const ushort ORE_RESOURCE_KIND = 2;
        public const ushort SPORE_POD_RESOURCE_KIND = 3;
        public const ushort CHEST_RESOURCE_KIND = 4;
        public const ushort WILDLIFE_SPAWN_KIND = 1;
        public const ushort HIGHLAND_SPAWN_KIND = 2;

        public static readonly Color DefaultTerrainMaterialColor = new(0.52f, 0.62f, 0.42f, 1f);
        public static readonly Color DebugWorldBoundsColor = new(1f, 0.82f, 0.18f, 1f);
        public static readonly Color DebugColliderChunkColor = new(1f, 0.28f, 0.18f, 1f);
        public static readonly Color DebugLod0ChunkColor = new(0.1f, 0.9f, 0.35f, 1f);
        public static readonly Color DebugLod1ChunkColor = new(0.18f, 0.65f, 1f, 1f);
        public static readonly Color DebugLod2ChunkColor = new(0.72f, 0.48f, 1f, 1f);
        public static readonly Color DebugLod3ChunkColor = new(0.75f, 0.75f, 0.75f, 1f);

        public static WorldGenerationRequest CreateDefaultRequest()
        {
            return new WorldGenerationRequest(
                new WorldGenerationSeed(DEFAULT_WORLD_SEED),
                WorldChunkBounds.Default,
                DEFAULT_CHUNK_WORLD_SIZE,
                DEFAULT_BASE_QUAD_COUNT,
                DEFAULT_LOD,
                DEFAULT_ADD_SKIRTS,
                DEFAULT_SKIRT_DEPTH);
        }
    }
}
