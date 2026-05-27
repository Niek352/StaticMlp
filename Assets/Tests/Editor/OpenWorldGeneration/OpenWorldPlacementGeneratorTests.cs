using System.Collections.Generic;
using NUnit.Framework;
using StaticMlp.Features.OpenWorldGeneration;
using UnityEngine;

#pragma warning disable CS0618
namespace StaticMlp.Tests.OpenWorldGeneration
{
    public sealed class OpenWorldPlacementGeneratorTests
    {
        [Test]
        public void GenerateResourcePlacements_CanProducePhase2NodeKinds()
        {
            var request = new WorldGenerationRequest(
                new WorldGenerationSeed(137),
                new WorldChunkBounds(-24, 24, -24, 24),
                OpenWorldGenerationConfig.DEFAULT_CHUNK_WORLD_SIZE,
                32,
                0,
                false,
                0f);
            var sampler = new Phase2SurfaceSampler();
            var kinds = new HashSet<ushort>();

            for (var z = request.Bounds.MinZ; z <= request.Bounds.MaxZ; z++)
            for (var x = request.Bounds.MinX; x <= request.Bounds.MaxX; x++)
            {
                var placements = OpenWorldPlacementGenerator.GenerateResourcePlacements(
                    new WorldChunkId(x, z),
                    request,
                    sampler);

                for (var i = 0; i < placements.Length; i++)
                    kinds.Add(placements[i].KindId.Value);
            }

            Assert.That(kinds, Does.Contain((ushort)1));
            Assert.That(kinds, Does.Contain((ushort)2));
            Assert.That(kinds, Does.Contain((ushort)3));
            Assert.That(kinds, Does.Contain((ushort)4));
        }

        private sealed class Phase2SurfaceSampler : ISurfaceSampler
        {
            public SurfaceSample Sample(float worldX, float worldZ)
            {
                var bucket = Mathf.Abs(
                    Mathf.FloorToInt(worldX / OpenWorldGenerationConfig.DEFAULT_CHUNK_WORLD_SIZE)
                    + Mathf.FloorToInt(worldZ / OpenWorldGenerationConfig.DEFAULT_CHUNK_WORLD_SIZE) * 3) % 3;
                return bucket switch
                {
                    0 => CreateSample(biomeId: 1, materialId: 1, wetness: 0.1f),
                    1 => CreateSample(biomeId: 2, materialId: 1, wetness: 0.8f),
                    _ => CreateSample(biomeId: 3, materialId: 3, wetness: 0.1f)
                };
            }

            private static SurfaceSample CreateSample(byte biomeId, byte materialId, float wetness)
            {
                return new SurfaceSample(
                    0f,
                    Vector3.up,
                    biomeId,
                    materialId,
                    0f,
                    0f,
                    wetness);
            }
        }
    }
}
#pragma warning restore CS0618
