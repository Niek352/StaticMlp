using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using StaticMlp.LayerProcLite;

namespace StaticMlp.Features.OpenWorldGeneration.Jobs
{
    /// <summary>
    /// Burst job that generates deterministic ResourcePlacement and SpawnPlacement data for a chunk.
    /// </summary>
    [BurstCompile]
    public struct ResourcePlacementGenerationJob : IJob
    {
        public const int RESOURCE_CANDIDATES_PER_CHUNK = OpenWorldGenerationConfig.RESOURCE_CANDIDATES_PER_CHUNK;
        public const int SPAWN_CANDIDATES_PER_CHUNK = OpenWorldGenerationConfig.SPAWN_CANDIDATES_PER_CHUNK;
        public const int FALLBACK_GRID_SIZE = OpenWorldGenerationConfig.PLACEMENT_FALLBACK_GRID_SIZE;
        public const float MAX_WATER_MASK = OpenWorldGenerationConfig.PLACEMENT_MAX_WATER_MASK;
        public const float MIN_NORMAL_Y = OpenWorldGenerationConfig.PLACEMENT_MIN_NORMAL_Y;
        public const float RESOURCE_MARGIN_FRACTION = OpenWorldGenerationConfig.RESOURCE_MARGIN_FRACTION;
        public const float SPAWN_MARGIN_FRACTION = OpenWorldGenerationConfig.SPAWN_MARGIN_FRACTION;

        private static readonly ResourcePlacementKindId TREE_RESOURCE = new(OpenWorldGenerationConfig.TREE_RESOURCE_KIND);
        private static readonly ResourcePlacementKindId ORE_RESOURCE = new(OpenWorldGenerationConfig.ORE_RESOURCE_KIND);
        private static readonly ResourcePlacementKindId SPORE_POD_RESOURCE = new(OpenWorldGenerationConfig.SPORE_POD_RESOURCE_KIND);
        private static readonly ResourcePlacementKindId CHEST_RESOURCE = new(OpenWorldGenerationConfig.CHEST_RESOURCE_KIND);
        private static readonly SpawnPlacementKindId WILDLIFE_SPAWN = new(OpenWorldGenerationConfig.WILDLIFE_SPAWN_KIND);
        private static readonly SpawnPlacementKindId HIGHLAND_SPAWN = new(OpenWorldGenerationConfig.HIGHLAND_SPAWN_KIND);

        public WorldChunkId ChunkId;
        public LayerProcLiteChunkId LayerChunkId;
        public uint WorldSeed;
        public float ChunkWorldSize;
        public int Resolution;

        [ReadOnly] public NativeArray<OpenWorldNativeSurfaceSample>.ReadOnly Surfaces;

        public NativeArray<ResourcePlacement> ResourcePlacements;
        public NativeArray<int> ResourcePlacementCount;
        public NativeArray<SpawnPlacement> SpawnPlacements;
        public NativeArray<int> SpawnPlacementCount;

        public void Execute()
        {
            int resourceWritten = 0;
            int spawnWritten = 0;

            for (int i = 0; i < RESOURCE_CANDIDATES_PER_CHUNK; i++)
            {
                var candidate = CreateCandidate(
                    11,
                    i,
                    RESOURCE_MARGIN_FRACTION,
                    OpenWorldGenerationConfig.RESOURCE_SCALE_MIN,
                    OpenWorldGenerationConfig.RESOURCE_SCALE_MAX);
                if (!candidate.Valid)
                    continue;

                ResourcePlacements[resourceWritten++] = new ResourcePlacement(
                    LayerProcLiteDeterministicHash.CreateStablePositiveId(WorldSeed, LayerChunkId, 11, i),
                    SelectResourceKind(
                        candidate.Surface,
                        LayerProcLiteDeterministicHash.Unit(LayerProcLiteDeterministicHash.Hash(WorldSeed, LayerChunkId, 23, i))),
                    ChunkId,
                    new Vector3(candidate.Position.x, candidate.Position.y, candidate.Position.z),
                    candidate.YawDegrees,
                    candidate.Scale);
            }

            if (resourceWritten == 0
                && TryCreateFallbackCandidate(
                    17,
                    OpenWorldGenerationConfig.FALLBACK_RESOURCE_SCALE_MIN,
                    OpenWorldGenerationConfig.FALLBACK_RESOURCE_SCALE_MAX,
                    out var fallback))
            {
                ResourcePlacements[resourceWritten++] = new ResourcePlacement(
                    LayerProcLiteDeterministicHash.CreateStablePositiveId(WorldSeed, LayerChunkId, 17, 0),
                    SelectResourceKind(
                        fallback.Surface,
                        LayerProcLiteDeterministicHash.Unit(LayerProcLiteDeterministicHash.Hash(WorldSeed, LayerChunkId, 29, 0))),
                    ChunkId,
                    new Vector3(fallback.Position.x, fallback.Position.y, fallback.Position.z),
                    fallback.YawDegrees,
                    fallback.Scale);
            }

            for (int i = 0; i < SPAWN_CANDIDATES_PER_CHUNK; i++)
            {
                var candidate = CreateCandidate(
                    31,
                    i,
                    SPAWN_MARGIN_FRACTION,
                    OpenWorldGenerationConfig.SPAWN_SCALE_MIN,
                    OpenWorldGenerationConfig.SPAWN_SCALE_MAX);
                if (!candidate.Valid)
                    continue;

                SpawnPlacements[spawnWritten++] = new SpawnPlacement(
                    SelectSpawnKind(candidate.Surface),
                    ChunkId,
                    new Vector3(candidate.Position.x, candidate.Position.y, candidate.Position.z),
                    candidate.YawDegrees,
                    candidate.Scale);
            }

            if (spawnWritten == 0
                && TryCreateFallbackCandidate(
                    41,
                    OpenWorldGenerationConfig.SPAWN_SCALE_MIN,
                    OpenWorldGenerationConfig.SPAWN_SCALE_MAX,
                    out var fallbackSpawn))
            {
                SpawnPlacements[spawnWritten++] = new SpawnPlacement(
                    SelectSpawnKind(fallbackSpawn.Surface),
                    ChunkId,
                    new Vector3(fallbackSpawn.Position.x, fallbackSpawn.Position.y, fallbackSpawn.Position.z),
                    fallbackSpawn.YawDegrees,
                    fallbackSpawn.Scale);
            }

            ResourcePlacementCount[0] = resourceWritten;
            SpawnPlacementCount[0] = spawnWritten;
        }

        private OpenWorldNativePlacementCandidate CreateCandidate(
            int stream,
            int index,
            float marginFraction,
            float scaleMin,
            float scaleMax)
        {
            float margin = ChunkWorldSize * marginFraction;
            float usableSize = ChunkWorldSize - margin * 2f;
            if (usableSize <= 0f)
                return OpenWorldNativePlacementCandidate.Invalid;

            float originX = ChunkId.X * ChunkWorldSize;
            float originZ = ChunkId.Z * ChunkWorldSize;
            float x = originX + margin + usableSize * LayerProcLiteDeterministicHash.Unit(LayerProcLiteDeterministicHash.Hash(WorldSeed, LayerChunkId, stream, index));
            float z = originZ + margin + usableSize * LayerProcLiteDeterministicHash.Unit(LayerProcLiteDeterministicHash.Hash(WorldSeed, LayerChunkId, stream + 1, index));

            var surface = SampleSurface(x, z);
            if (surface.WaterMask > MAX_WATER_MASK || surface.Normal.y < MIN_NORMAL_Y)
                return OpenWorldNativePlacementCandidate.Invalid;

            return new OpenWorldNativePlacementCandidate(
                true,
                new float3(x, surface.Height, z),
                LayerProcLiteDeterministicHash.Unit(LayerProcLiteDeterministicHash.Hash(WorldSeed, LayerChunkId, stream + 2, index)) * 360f,
                math.lerp(scaleMin, scaleMax, LayerProcLiteDeterministicHash.Unit(LayerProcLiteDeterministicHash.Hash(WorldSeed, LayerChunkId, 13, index))),
                surface);
        }

        private bool TryCreateFallbackCandidate(
            int stream,
            float scaleMin,
            float scaleMax,
            out OpenWorldNativePlacementCandidate candidate)
        {
            float cellSize = ChunkWorldSize / FALLBACK_GRID_SIZE;
            float originX = ChunkId.X * ChunkWorldSize;
            float originZ = ChunkId.Z * ChunkWorldSize;
            int start = (int)(LayerProcLiteDeterministicHash.Hash(WorldSeed, LayerChunkId, stream, 0) % (FALLBACK_GRID_SIZE * FALLBACK_GRID_SIZE));

            for (int i = 0; i < FALLBACK_GRID_SIZE * FALLBACK_GRID_SIZE; i++)
            {
                int cell = (start + i) % (FALLBACK_GRID_SIZE * FALLBACK_GRID_SIZE);
                int xIndex = cell % FALLBACK_GRID_SIZE;
                int zIndex = cell / FALLBACK_GRID_SIZE;
                float x = originX + (xIndex + 0.5f) * cellSize;
                float z = originZ + (zIndex + 0.5f) * cellSize;

                var surface = SampleSurface(x, z);
                if (surface.WaterMask > MAX_WATER_MASK || surface.Normal.y < MIN_NORMAL_Y)
                    continue;

                candidate = new OpenWorldNativePlacementCandidate(
                    true,
                    new float3(x, surface.Height, z),
                    LayerProcLiteDeterministicHash.Unit(LayerProcLiteDeterministicHash.Hash(WorldSeed, LayerChunkId, stream + 1, cell)) * 360f,
                    math.lerp(
                        scaleMin,
                        scaleMax,
                        LayerProcLiteDeterministicHash.Unit(LayerProcLiteDeterministicHash.Hash(WorldSeed, LayerChunkId, 43, 0))),
                    surface);
                return true;
            }

            candidate = OpenWorldNativePlacementCandidate.Invalid;
            return false;
        }

        private OpenWorldNativeSurfaceSample SampleSurface(float worldX, float worldZ)
        {
            float localXf = (worldX - ChunkId.X * ChunkWorldSize) / ChunkWorldSize * (Resolution - 1);
            float localZf = (worldZ - ChunkId.Z * ChunkWorldSize) / ChunkWorldSize * (Resolution - 1);

            int x0 = (int)math.clamp(math.floor(localXf), 0, Resolution - 1);
            int z0 = (int)math.clamp(math.floor(localZf), 0, Resolution - 1);
            int x1 = math.min(x0 + 1, Resolution - 1);
            int z1 = math.min(z0 + 1, Resolution - 1);
            float tx = localXf - x0;
            float tz = localZf - z0;

            var s00 = Surfaces[LayerProcLiteGrid.ToIndex(x0, z0, Resolution)];
            var s10 = Surfaces[LayerProcLiteGrid.ToIndex(x1, z0, Resolution)];
            var s01 = Surfaces[LayerProcLiteGrid.ToIndex(x0, z1, Resolution)];
            var s11 = Surfaces[LayerProcLiteGrid.ToIndex(x1, z1, Resolution)];
            var nearest = Surfaces[LayerProcLiteGrid.ToIndex(
                (int)math.clamp(math.round(localXf), 0, Resolution - 1),
                (int)math.clamp(math.round(localZf), 0, Resolution - 1),
                Resolution)];

            return new OpenWorldNativeSurfaceSample(
                LayerProcLiteMath.Bilinear(s00.Height, s10.Height, s01.Height, s11.Height, tx, tz),
                math.normalize(LayerProcLiteMath.Bilinear(s00.Normal, s10.Normal, s01.Normal, s11.Normal, tx, tz)),
                nearest.BiomeId,
                nearest.PrimaryMaterialId,
                LayerProcLiteMath.Bilinear(s00.RoadMask, s10.RoadMask, s01.RoadMask, s11.RoadMask, tx, tz),
                LayerProcLiteMath.Bilinear(s00.WaterMask, s10.WaterMask, s01.WaterMask, s11.WaterMask, tx, tz),
                LayerProcLiteMath.Bilinear(s00.Wetness, s10.Wetness, s01.Wetness, s11.Wetness, tx, tz));
        }

        private static ResourcePlacementKindId SelectResourceKind(OpenWorldNativeSurfaceSample sample, float chestRoll)
        {
            if (chestRoll < OpenWorldGenerationConfig.CHEST_PLACEMENT_CHANCE)
                return CHEST_RESOURCE;
            if (sample.BiomeId == 2 || sample.Wetness >= OpenWorldGenerationConfig.SPORE_MIN_WETNESS)
                return SPORE_POD_RESOURCE;
            if (sample.BiomeId == 3 || sample.PrimaryMaterialId >= 2)
                return ORE_RESOURCE;

            return TREE_RESOURCE;
        }

        private static SpawnPlacementKindId SelectSpawnKind(OpenWorldNativeSurfaceSample sample)
        {
            return sample.BiomeId == 3 ? HIGHLAND_SPAWN : WILDLIFE_SPAWN;
        }
    }
}
