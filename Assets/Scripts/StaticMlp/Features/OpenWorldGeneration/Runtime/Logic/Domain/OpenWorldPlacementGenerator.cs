using System;
using System.Collections.Generic;
using UnityEngine;

namespace StaticMlp.Features.OpenWorldGeneration
{
    [Obsolete("Use OpenWorldChunkGenerationSystem instead")]
    public static class OpenWorldPlacementGenerator
    {
        public const float MAX_WATER_MASK = 0.35f;
        public const float MIN_NORMAL_Y = 0.85f;

        private const int RESOURCE_CANDIDATES_PER_CHUNK = 24;
        private const int SPAWN_CANDIDATES_PER_CHUNK = 8;
        private const int FALLBACK_GRID_SIZE = 16;
        private const float RESOURCE_MARGIN_FRACTION = 0.08f;
        private const float SPAWN_MARGIN_FRACTION = 0.18f;
        private static readonly ResourcePlacementKindId WOOD_RESOURCE = new(1);
        private static readonly ResourcePlacementKindId STONE_RESOURCE = new(2);
        private static readonly SpawnPlacementKindId WILDLIFE_SPAWN = new(1);
        private static readonly SpawnPlacementKindId HIGHLAND_SPAWN = new(2);

        public static ResourcePlacement[] GenerateResourcePlacements(
            WorldChunkId chunkId,
            WorldGenerationRequest request,
            ISurfaceSampler surfaceSampler)
        {
            if (surfaceSampler == null)
                throw new ArgumentNullException(nameof(surfaceSampler));

            var placements = new List<ResourcePlacement>(RESOURCE_CANDIDATES_PER_CHUNK);
            for (var i = 0; i < RESOURCE_CANDIDATES_PER_CHUNK; i++)
            {
                var candidate = CreateCandidate(chunkId, request, surfaceSampler, 11, i, RESOURCE_MARGIN_FRACTION);
                if (!candidate.Valid)
                    continue;

                placements.Add(new ResourcePlacement(
                    CreatePlacementId(request.Seed.Value, chunkId, 11, i),
                    SelectResourceKind(candidate.Sample),
                    chunkId,
                    candidate.Position,
                    candidate.YawDegrees,
                    Mathf.Lerp(0.8f, 1.35f, Unit(Hash(request.Seed.Value, chunkId, 13, i)))));
            }

            if (placements.Count == 0
                && TryCreateFallbackCandidate(chunkId, request, surfaceSampler, 17, out var fallback))
            {
                placements.Add(new ResourcePlacement(
                    CreatePlacementId(request.Seed.Value, chunkId, 17, 0),
                    SelectResourceKind(fallback.Sample),
                    chunkId,
                    fallback.Position,
                    fallback.YawDegrees,
                    Mathf.Lerp(0.8f, 1.35f, Unit(Hash(request.Seed.Value, chunkId, 19, 0)))));
            }

            return placements.ToArray();
        }

        public static SpawnPlacement[] GenerateSpawnPlacements(
            WorldChunkId chunkId,
            WorldGenerationRequest request,
            ISurfaceSampler surfaceSampler)
        {
            if (surfaceSampler == null)
                throw new ArgumentNullException(nameof(surfaceSampler));

            var placements = new List<SpawnPlacement>(SPAWN_CANDIDATES_PER_CHUNK);
            for (var i = 0; i < SPAWN_CANDIDATES_PER_CHUNK; i++)
            {
                var candidate = CreateCandidate(chunkId, request, surfaceSampler, 31, i, SPAWN_MARGIN_FRACTION);
                if (!candidate.Valid)
                    continue;

                placements.Add(new SpawnPlacement(
                    SelectSpawnKind(candidate.Sample),
                    chunkId,
                    candidate.Position,
                    candidate.YawDegrees,
                    Mathf.Lerp(0.9f, 1.15f, Unit(Hash(request.Seed.Value, chunkId, 37, i)))));
            }

            if (placements.Count == 0
                && TryCreateFallbackCandidate(chunkId, request, surfaceSampler, 41, out var fallback))
            {
                placements.Add(new SpawnPlacement(
                    SelectSpawnKind(fallback.Sample),
                    chunkId,
                    fallback.Position,
                    fallback.YawDegrees,
                    Mathf.Lerp(0.9f, 1.15f, Unit(Hash(request.Seed.Value, chunkId, 43, 0)))));
            }

            return placements.ToArray();
        }

        private static PlacementCandidate CreateCandidate(
            WorldChunkId chunkId,
            WorldGenerationRequest request,
            ISurfaceSampler surfaceSampler,
            int stream,
            int index,
            float marginFraction)
        {
            var margin = request.ChunkWorldSize * marginFraction;
            var usableSize = request.ChunkWorldSize - margin * 2f;
            if (usableSize <= 0f)
                throw new ArgumentOutOfRangeException(nameof(request.ChunkWorldSize), request.ChunkWorldSize, "Chunk world size is too small for placement generation.");

            var originX = chunkId.X * request.ChunkWorldSize;
            var originZ = chunkId.Z * request.ChunkWorldSize;
            var x = originX + margin + usableSize * Unit(Hash(request.Seed.Value, chunkId, stream, index));
            var z = originZ + margin + usableSize * Unit(Hash(request.Seed.Value, chunkId, stream + 1, index));
            var sample = surfaceSampler.Sample(x, z);
            if (sample.WaterMask > MAX_WATER_MASK || sample.Normal.y < MIN_NORMAL_Y)
                return PlacementCandidate.Invalid;

            return new PlacementCandidate(
                true,
                new Vector3(x, sample.Height, z),
                Unit(Hash(request.Seed.Value, chunkId, stream + 2, index)) * 360f,
                sample);
        }

        private static bool TryCreateFallbackCandidate(
            WorldChunkId chunkId,
            WorldGenerationRequest request,
            ISurfaceSampler surfaceSampler,
            int stream,
            out PlacementCandidate candidate)
        {
            var cellSize = request.ChunkWorldSize / FALLBACK_GRID_SIZE;
            var originX = chunkId.X * request.ChunkWorldSize;
            var originZ = chunkId.Z * request.ChunkWorldSize;
            var start = (int)(Hash(request.Seed.Value, chunkId, stream, 0) % (FALLBACK_GRID_SIZE * FALLBACK_GRID_SIZE));

            for (var i = 0; i < FALLBACK_GRID_SIZE * FALLBACK_GRID_SIZE; i++)
            {
                var cell = (start + i) % (FALLBACK_GRID_SIZE * FALLBACK_GRID_SIZE);
                var xIndex = cell % FALLBACK_GRID_SIZE;
                var zIndex = cell / FALLBACK_GRID_SIZE;
                var x = originX + (xIndex + 0.5f) * cellSize;
                var z = originZ + (zIndex + 0.5f) * cellSize;
                var sample = surfaceSampler.Sample(x, z);
                if (sample.WaterMask > MAX_WATER_MASK || sample.Normal.y < MIN_NORMAL_Y)
                    continue;

                candidate = new PlacementCandidate(
                    true,
                    new Vector3(x, sample.Height, z),
                    Unit(Hash(request.Seed.Value, chunkId, stream + 1, cell)) * 360f,
                    sample);
                return true;
            }

            candidate = PlacementCandidate.Invalid;
            return false;
        }

        private static ResourcePlacementKindId SelectResourceKind(SurfaceSample sample)
        {
            return sample.PrimaryMaterialId >= 3 || sample.BiomeId == 3 ? STONE_RESOURCE : WOOD_RESOURCE;
        }

        private static SpawnPlacementKindId SelectSpawnKind(SurfaceSample sample)
        {
            return sample.BiomeId == 3 ? HIGHLAND_SPAWN : WILDLIFE_SPAWN;
        }

        private static uint Hash(int seed, WorldChunkId chunkId, int stream, int index)
        {
            unchecked
            {
                var hash = (uint)seed;
                hash ^= (uint)chunkId.X * 0x9E3779B9u;
                hash = RotateLeft(hash, 13);
                hash ^= (uint)chunkId.Z * 0x85EBCA6Bu;
                hash = RotateLeft(hash, 17);
                hash ^= (uint)stream * 0xC2B2AE35u;
                hash = RotateLeft(hash, 11);
                hash ^= (uint)index * 0x27D4EB2Fu;
                hash ^= hash >> 15;
                hash *= 0x2C1B3C6Du;
                hash ^= hash >> 12;
                hash *= 0x297A2D39u;
                hash ^= hash >> 15;
                return hash;
            }
        }

        private static long CreatePlacementId(int seed, WorldChunkId chunkId, int stream, int index)
        {
            unchecked
            {
                var high = Hash(seed, chunkId, stream + 101, index);
                var low = Hash(seed, chunkId, stream + 211, index);
                var value = ((long)high << 32) | low;
                value &= long.MaxValue;
                return value == 0 ? 1 : value;
            }
        }

        private static uint RotateLeft(uint value, int count)
        {
            return (value << count) | (value >> (32 - count));
        }

        private static float Unit(uint hash)
        {
            return (hash & 0x00FFFFFFu) / 16777216f;
        }

        private readonly struct PlacementCandidate
        {
            public static readonly PlacementCandidate Invalid = new(false, default, 0f, default);

            public PlacementCandidate(bool valid, Vector3 position, float yawDegrees, SurfaceSample sample)
            {
                Valid = valid;
                Position = position;
                YawDegrees = yawDegrees;
                Sample = sample;
            }

            public readonly bool Valid;
            public readonly Vector3 Position;
            public readonly float YawDegrees;
            public readonly SurfaceSample Sample;
        }
    }
}
