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
        public const int RESOURCE_CANDIDATES_PER_CHUNK = 24;
        public const int SPAWN_CANDIDATES_PER_CHUNK = 8;
        public const int FALLBACK_GRID_SIZE = 16;
        public const float MAX_WATER_MASK = 0.35f;
        public const float MIN_NORMAL_Y = 0.85f;
        public const float RESOURCE_MARGIN_FRACTION = 0.08f;
        public const float SPAWN_MARGIN_FRACTION = 0.18f;

        private static readonly ResourcePlacementKindId WOOD_RESOURCE = new(1);
        private static readonly ResourcePlacementKindId STONE_RESOURCE = new(2);
        private static readonly SpawnPlacementKindId WILDLIFE_SPAWN = new(1);
        private static readonly SpawnPlacementKindId HIGHLAND_SPAWN = new(2);

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
                var candidate = CreateCandidate(11, i, RESOURCE_MARGIN_FRACTION);
                if (!candidate.Valid)
                    continue;

                ResourcePlacements[resourceWritten++] = new ResourcePlacement(
                    LayerProcLiteDeterministicHash.CreateStablePositiveId(WorldSeed, LayerChunkId, 11, i),
                    SelectResourceKind(candidate.Surface),
                    ChunkId,
                    new Vector3(candidate.Position.x, candidate.Position.y, candidate.Position.z),
                    candidate.YawDegrees,
                    candidate.Scale);
            }

            if (resourceWritten == 0 && TryCreateFallbackCandidate(17, out var fallback))
            {
                ResourcePlacements[resourceWritten++] = new ResourcePlacement(
                    LayerProcLiteDeterministicHash.CreateStablePositiveId(WorldSeed, LayerChunkId, 17, 0),
                    SelectResourceKind(fallback.Surface),
                    ChunkId,
                    new Vector3(fallback.Position.x, fallback.Position.y, fallback.Position.z),
                    fallback.YawDegrees,
                    fallback.Scale);
            }

            for (int i = 0; i < SPAWN_CANDIDATES_PER_CHUNK; i++)
            {
                var candidate = CreateCandidate(31, i, SPAWN_MARGIN_FRACTION);
                if (!candidate.Valid)
                    continue;

                SpawnPlacements[spawnWritten++] = new SpawnPlacement(
                    SelectSpawnKind(candidate.Surface),
                    ChunkId,
                    new Vector3(candidate.Position.x, candidate.Position.y, candidate.Position.z),
                    candidate.YawDegrees,
                    candidate.Scale);
            }

            if (spawnWritten == 0 && TryCreateFallbackCandidate(41, out var fallbackSpawn))
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

        private OpenWorldNativePlacementCandidate CreateCandidate(int stream, int index, float marginFraction)
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
                math.lerp(0.8f, 1.35f, LayerProcLiteDeterministicHash.Unit(LayerProcLiteDeterministicHash.Hash(WorldSeed, LayerChunkId, 13, index))),
                surface);
        }

        private bool TryCreateFallbackCandidate(int stream, out OpenWorldNativePlacementCandidate candidate)
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
                    math.lerp(0.9f, 1.15f, LayerProcLiteDeterministicHash.Unit(LayerProcLiteDeterministicHash.Hash(WorldSeed, LayerChunkId, 43, 0))),
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

        private static ResourcePlacementKindId SelectResourceKind(OpenWorldNativeSurfaceSample sample)
        {
            return sample.PrimaryMaterialId >= 3 || sample.BiomeId == 3 ? STONE_RESOURCE : WOOD_RESOURCE;
        }

        private static SpawnPlacementKindId SelectSpawnKind(OpenWorldNativeSurfaceSample sample)
        {
            return sample.BiomeId == 3 ? HIGHLAND_SPAWN : WILDLIFE_SPAWN;
        }
    }
}
