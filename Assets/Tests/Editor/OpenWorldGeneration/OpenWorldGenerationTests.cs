using System;
using NUnit.Framework;
using StaticMlp.Features.OpenWorldGeneration;
using UnityEngine;

namespace StaticMlp.Tests.OpenWorldGeneration
{
    public sealed class OpenWorldGenerationTests
    {
        private const int LOD0_QUADS = 64;
        private const float MAX_NEAR_BORDER_HEIGHT_STEP = 1.5f;
        private const float MAX_INTERIOR_NORMAL_ANGLE = 2f;

        [Test]
        public void GenerateChunk_WhenSeedAndChunkMatch_IsDeterministic()
        {
            var service = new SimpleWorldGenerationService();
            var request = CreateRequest(new WorldGenerationSeed(11), 0, false);

            var first = service.GenerateChunk(new WorldChunkId(2, -3), request).TerrainMesh;
            var second = service.GenerateChunk(new WorldChunkId(2, -3), request).TerrainMesh;

            Assert.That(second.Vertices.Length, Is.EqualTo(first.Vertices.Length));
            for (var i = 0; i < first.Vertices.Length; i++)
                Assert.That(second.Vertices[i], Is.EqualTo(first.Vertices[i]));
        }

        [Test]
        public void GenerateChunk_WhenSeedChanges_ChangesHeightOutput()
        {
            var service = new SimpleWorldGenerationService();
            var first = service.GenerateChunk(new WorldChunkId(0, 0), CreateRequest(new WorldGenerationSeed(11), 0, false)).TerrainMesh;
            var second = service.GenerateChunk(new WorldChunkId(0, 0), CreateRequest(new WorldGenerationSeed(12), 0, false)).TerrainMesh;

            var anyDifferent = false;
            for (var i = 0; i < first.Vertices.Length; i++)
            {
                if (Mathf.Abs(first.Vertices[i].y - second.Vertices[i].y) <= 0.001f)
                    continue;

                anyDifferent = true;
                break;
            }

            Assert.That(anyDifferent, Is.True);
        }

        [Test]
        public void GenerateChunk_Placements_WhenSeedAndChunkMatch_AreDeterministic()
        {
            var service = new SimpleWorldGenerationService();
            var request = CreateRequest(new WorldGenerationSeed(12345), 0, false);

            var first = service.GenerateChunk(new WorldChunkId(0, 0), request);
            var second = service.GenerateChunk(new WorldChunkId(0, 0), request);

            Assert.That(first.ResourcePlacements, Is.Not.Empty);
            Assert.That(first.SpawnPlacements, Is.Not.Empty);
            AssertResourcePlacementsEqual(first.ResourcePlacements, second.ResourcePlacements);
            AssertSpawnPlacementsEqual(first.SpawnPlacements, second.SpawnPlacements);
        }

        [Test]
        public void GenerateChunk_Placements_WhenSeedChanges_ChangesPlacementOutput()
        {
            var service = new SimpleWorldGenerationService();
            var first = service.GenerateChunk(new WorldChunkId(0, 0), CreateRequest(new WorldGenerationSeed(11), 0, false));
            var second = service.GenerateChunk(new WorldChunkId(0, 0), CreateRequest(new WorldGenerationSeed(12), 0, false));

            Assert.That(
                ResourcePlacementsDiffer(first.ResourcePlacements, second.ResourcePlacements)
                || SpawnPlacementsDiffer(first.SpawnPlacements, second.SpawnPlacements),
                Is.True);
        }

        [Test]
        public void GenerateChunk_Placements_AreInsideRequestedChunk()
        {
            var service = new SimpleWorldGenerationService();
            var chunkId = new WorldChunkId(-2, 3);
            var request = CreateRequest(new WorldGenerationSeed(42), 0, false);
            var generated = service.GenerateChunk(chunkId, request);

            Assert.That(generated.ResourcePlacements, Is.Not.Empty);
            Assert.That(generated.SpawnPlacements, Is.Not.Empty);
            AssertPlacementsInsideChunk(generated.ResourcePlacements, chunkId, request.ChunkWorldSize);
            AssertPlacementsInsideChunk(generated.SpawnPlacements, chunkId, request.ChunkWorldSize);
        }

        [Test]
        public void GenerateChunk_Placements_SkipWaterAndSteepTerrain()
        {
            var service = new SimpleWorldGenerationService();
            var chunkId = new WorldChunkId(0, 0);
            var request = CreateRequest(new WorldGenerationSeed(12345), 0, false);
            var sampler = new SimpleSurfaceSampler(request.Seed);
            var generated = service.GenerateChunk(chunkId, request);

            AssertPlacementSamplesAreValid(generated.ResourcePlacements, sampler);
            AssertPlacementSamplesAreValid(generated.SpawnPlacements, sampler);
        }

        [Test]
        public void LayerProcGenGenerateChunk_Placements_WhenSeedAndChunkMatch_AreDeterministic()
        {
            using var service = new LayerProcGenWorldGenerationService();
            var request = CreateRequest(new WorldGenerationSeed(12345), 0, false);

            var first = service.GenerateChunk(new WorldChunkId(-7, -8), request);
            var second = service.GenerateChunk(new WorldChunkId(-7, -8), request);

            Assert.That(first.ResourcePlacements, Is.Not.Empty);
            Assert.That(first.SpawnPlacements, Is.Not.Empty);
            AssertResourcePlacementsEqual(first.ResourcePlacements, second.ResourcePlacements);
            AssertSpawnPlacementsEqual(first.SpawnPlacements, second.SpawnPlacements);
        }

        [Test]
        public void LayerProcGenGenerateChunk_WhenSeedAndChunkMatch_IsDeterministic()
        {
            using var service = new LayerProcGenWorldGenerationService();
            var request = CreateRequest(new WorldGenerationSeed(11), 0, false);

            var first = service.GenerateChunk(new WorldChunkId(2, -3), request).TerrainMesh;
            var second = service.GenerateChunk(new WorldChunkId(2, -3), request).TerrainMesh;

            Assert.That(second.Vertices.Length, Is.EqualTo(first.Vertices.Length));
            for (var i = 0; i < first.Vertices.Length; i++)
                Assert.That(second.Vertices[i], Is.EqualTo(first.Vertices[i]));
        }

        [Test]
        public void LayerProcGenGenerateChunk_WhenSeedChanges_ChangesHeightOutput()
        {
            TerrainMeshData first;
            using (var service = new LayerProcGenWorldGenerationService())
                first = service.GenerateChunk(new WorldChunkId(0, 0), CreateRequest(new WorldGenerationSeed(11), 0, false)).TerrainMesh;

            TerrainMeshData second;
            using (var service = new LayerProcGenWorldGenerationService())
                second = service.GenerateChunk(new WorldChunkId(0, 0), CreateRequest(new WorldGenerationSeed(12), 0, false)).TerrainMesh;

            Assert.That(AnyHeightDifferent(first, second), Is.True);
        }

        [Test]
        public void GenerateChunk_NeighborChunks_ShareBorderHeights()
        {
            var service = new SimpleWorldGenerationService();
            var request = CreateRequest(new WorldGenerationSeed(42), 0, false);
            var left = service.GenerateChunk(new WorldChunkId(0, 0), request).TerrainMesh;
            var right = service.GenerateChunk(new WorldChunkId(1, 0), request).TerrainMesh;
            const int quads = 64;

            for (var z = 0; z <= quads; z++)
            {
                var leftHeight = left.Vertices[GridIndex(quads, z, quads)].y;
                var rightHeight = right.Vertices[GridIndex(0, z, quads)].y;
                Assert.That(Mathf.Abs(leftHeight - rightHeight), Is.LessThan(0.0001f));
            }
        }

        [Test]
        public void GenerateChunk_NeighborChunksAtDifferentLods_ShareMatchingBorderHeights()
        {
            var service = new SimpleWorldGenerationService();
            var left = service.GenerateChunk(new WorldChunkId(0, 0), CreateRequest(new WorldGenerationSeed(42), 0, false)).TerrainMesh;
            var right = service.GenerateChunk(new WorldChunkId(1, 0), CreateRequest(new WorldGenerationSeed(42), 1, false)).TerrainMesh;
            const int leftQuads = 64;
            const int rightQuads = 32;

            for (var z = 0; z <= rightQuads; z++)
            {
                var leftHeight = left.Vertices[GridIndex(leftQuads, z * 2, leftQuads)].y;
                var rightHeight = right.Vertices[GridIndex(0, z, rightQuads)].y;
                Assert.That(Mathf.Abs(leftHeight - rightHeight), Is.LessThan(0.0001f));
            }
        }

        [Test]
        public void LayerProcGenGenerateChunk_NeighborChunks_ShareBorderHeights()
        {
            using var service = new LayerProcGenWorldGenerationService();
            var request = CreateRequest(new WorldGenerationSeed(42), 0, false);
            var left = service.GenerateChunk(new WorldChunkId(0, 0), request).TerrainMesh;
            var right = service.GenerateChunk(new WorldChunkId(1, 0), request).TerrainMesh;
            const int quads = 64;

            for (var z = 0; z <= quads; z++)
            {
                var leftHeight = left.Vertices[GridIndex(quads, z, quads)].y;
                var rightHeight = right.Vertices[GridIndex(0, z, quads)].y;
                Assert.That(Mathf.Abs(leftHeight - rightHeight), Is.LessThan(0.0001f));
            }
        }

        [Test]
        public void LayerProcGenGenerateChunk_NeighborChunksAtDifferentLods_ShareMatchingBorderHeights()
        {
            using var service = new LayerProcGenWorldGenerationService();
            var left = service.GenerateChunk(new WorldChunkId(0, 0), CreateRequest(new WorldGenerationSeed(42), 0, false)).TerrainMesh;
            var right = service.GenerateChunk(new WorldChunkId(1, 0), CreateRequest(new WorldGenerationSeed(42), 1, false)).TerrainMesh;
            const int leftQuads = 64;
            const int rightQuads = 32;

            for (var z = 0; z <= rightQuads; z++)
            {
                var leftHeight = left.Vertices[GridIndex(leftQuads, z * 2, leftQuads)].y;
                var rightHeight = right.Vertices[GridIndex(0, z, rightQuads)].y;
                Assert.That(Mathf.Abs(leftHeight - rightHeight), Is.LessThan(0.0001f));
            }
        }

        [Test]
        public void LayerProcGenGenerateChunk_AdjacentChunks_DoNotCreateNearBorderHeightSteps()
        {
            AssertLayerProcGenNearBorderContinuity(new WorldGenerationSeed(11), new WorldChunkId(0, 0), SeamAxis.PositiveX);
            AssertLayerProcGenNearBorderContinuity(new WorldGenerationSeed(42), new WorldChunkId(-1, 0), SeamAxis.PositiveX);
            AssertLayerProcGenNearBorderContinuity(new WorldGenerationSeed(12345), new WorldChunkId(-2, -1), SeamAxis.PositiveX);
            AssertLayerProcGenNearBorderContinuity(new WorldGenerationSeed(11), new WorldChunkId(0, 0), SeamAxis.PositiveZ);
            AssertLayerProcGenNearBorderContinuity(new WorldGenerationSeed(42), new WorldChunkId(0, -1), SeamAxis.PositiveZ);
            AssertLayerProcGenNearBorderContinuity(new WorldGenerationSeed(12345), new WorldChunkId(-2, -2), SeamAxis.PositiveZ);
        }

        [Test]
        public void LayerProcGenGenerateChunk_InteriorNormals_DoNotCreateAbruptCreases()
        {
            AssertLayerProcGenInteriorNormalContinuity(new WorldGenerationSeed(11), new WorldChunkId(0, 0));
            AssertLayerProcGenInteriorNormalContinuity(new WorldGenerationSeed(42), new WorldChunkId(-1, 0));
            AssertLayerProcGenInteriorNormalContinuity(new WorldGenerationSeed(12345), new WorldChunkId(-2, -1));
        }

        [Test]
        public void FromWorldPosition_UsesFloorForNegativeCoordinates()
        {
            Assert.That(WorldChunkId.FromWorldPosition(-0.01f, -0.01f, 128f), Is.EqualTo(new WorldChunkId(-1, -1)));
            Assert.That(WorldChunkId.FromWorldPosition(-128f, -128.01f, 128f), Is.EqualTo(new WorldChunkId(-1, -2)));
            Assert.That(WorldChunkId.FromWorldPosition(0f, 127.99f, 128f), Is.EqualTo(new WorldChunkId(0, 0)));
        }

        [TestCase(0, 65)]
        [TestCase(1, 33)]
        [TestCase(2, 17)]
        [TestCase(3, 9)]
        public void TerrainMeshBuilder_BuildsExpectedLodVertexGrid(int lod, int expectedSide)
        {
            var mesh = TerrainMeshBuilder.Build(
                new TerrainMeshBuildRequest(new WorldChunkId(0, 0), 128f, lod, 64, false, 1f),
                new FlatSurfaceSampler());

            Assert.That(mesh.Vertices.Length, Is.EqualTo(expectedSide * expectedSide));
        }

        [Test]
        public void GenerateChunk_WhenChunkOutsideBounds_Throws()
        {
            var service = new SimpleWorldGenerationService();
            var request = CreateRequest(new WorldGenerationSeed(1), 0, false);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => service.GenerateChunk(new WorldChunkId(8, 0), request));
        }

        [Test]
        public void LayerProcGenGenerateChunk_WhenChunkOutsideBounds_Throws()
        {
            using var service = new LayerProcGenWorldGenerationService();
            var request = CreateRequest(new WorldGenerationSeed(1), 0, false);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => service.GenerateChunk(new WorldChunkId(8, 0), request));
        }

        [Test]
        public void LayerProcGenGenerateChunk_WhenRequestSettingsChange_Throws()
        {
            using var service = new LayerProcGenWorldGenerationService();
            service.GenerateChunk(new WorldChunkId(0, 0), CreateRequest(new WorldGenerationSeed(1), 0, false));

            Assert.Throws<System.InvalidOperationException>(() =>
                service.GenerateChunk(new WorldChunkId(0, 0), CreateRequest(new WorldGenerationSeed(2), 0, false)));
        }

        [Test]
        public void TerrainMeshBuilder_WhenSkirtsEnabled_AddsExpectedGeometry()
        {
            const int quads = 64;
            var noSkirts = TerrainMeshBuilder.Build(
                new TerrainMeshBuildRequest(new WorldChunkId(0, 0), 128f, 0, quads, false, 1f),
                new FlatSurfaceSampler());
            var skirts = TerrainMeshBuilder.Build(
                new TerrainMeshBuildRequest(new WorldChunkId(0, 0), 128f, 0, quads, true, 4f),
                new FlatSurfaceSampler());

            Assert.That(skirts.Vertices.Length - noSkirts.Vertices.Length, Is.EqualTo(4 * (quads + 1)));
            Assert.That(skirts.Triangles.Length - noSkirts.Triangles.Length, Is.EqualTo(4 * quads * 6));
        }

        [Test]
        public void TerrainRuntime_DebugSnapshot_ReportsLoadedChunksAndColliderState()
        {
            var config = CreateRuntimeConfig("OpenWorldTerrainDebugSnapshotTest");
            config.Bounds = new WorldChunkBounds(-1, 1, -1, 1);
            config.ViewRadiusInChunks = 1;
            config.ColliderRadiusInChunks = 0;
            var runtime = OpenWorldTerrainRuntime.Create(config);

            try
            {
                runtime.StreamAround(Vector3.zero);

                var snapshot = runtime.CreateDebugSnapshot();

                Assert.That(snapshot.Seed, Is.EqualTo(config.Seed));
                Assert.That(snapshot.Bounds, Is.EqualTo(config.Bounds));
                Assert.That(snapshot.HasFocusChunk, Is.True);
                Assert.That(snapshot.FocusChunk, Is.EqualTo(new WorldChunkId(0, 0)));
                Assert.That(snapshot.LoadedChunkCount, Is.EqualTo(9));
                Assert.That(snapshot.ColliderChunkCount, Is.EqualTo(1));
                Assert.That(snapshot.CountLod(0), Is.EqualTo(9));
                Assert.That(snapshot.Chunks[0].ChunkId, Is.EqualTo(new WorldChunkId(-1, -1)));
                Assert.That(snapshot.Chunks[0].WorldOrigin, Is.EqualTo(new Vector3(-128f, 0f, -128f)));
            }
            finally
            {
                runtime.Dispose();
            }
        }

        [Test]
        public void TerrainRuntime_StreamAround_RespectsMaxChunkLoadsPerFrame()
        {
            var config = CreateRuntimeConfig("OpenWorldTerrainFrameBudgetTest");
            config.Bounds = new WorldChunkBounds(-1, 1, -1, 1);
            config.ViewRadiusInChunks = 1;
            config.MaxChunkLoadsPerFrame = 2;
            var runtime = OpenWorldTerrainRuntime.Create(config);

            try
            {
                runtime.StreamAround(Vector3.zero);
                Assert.That(runtime.CreateDebugSnapshot().LoadedChunkCount, Is.EqualTo(2));

                runtime.StreamAround(Vector3.zero);
                Assert.That(runtime.CreateDebugSnapshot().LoadedChunkCount, Is.EqualTo(4));
            }
            finally
            {
                runtime.Dispose();
            }
        }

        [Test]
        public void TerrainRuntime_Dispose_DestroysRootChunksAndMeshesInEditMode()
        {
            const string rootName = "OpenWorldTerrainCleanupTest";
            var config = CreateRuntimeConfig(rootName);
            config.Bounds = new WorldChunkBounds(0, 0, 0, 0);
            config.ViewRadiusInChunks = 0;
            var runtime = OpenWorldTerrainRuntime.Create(config);
            runtime.StreamAround(Vector3.zero);

            Assert.That(GameObject.Find(rootName), Is.Not.Null);
            Assert.That(UnityEngine.Object.FindObjectsOfType<TerrainChunkView>(), Is.Not.Empty);

            runtime.Dispose();

            Assert.That(GameObject.Find(rootName), Is.Null);
            Assert.That(UnityEngine.Object.FindObjectsOfType<TerrainChunkView>(), Is.Empty);
        }

        [Test]
        public void GeneratedChunkData_LegacyConstructor_UsesEmptyPlacementArrays()
        {
            var mesh = new TerrainMeshData(
                Array.Empty<Vector3>(),
                Array.Empty<Vector3>(),
                Array.Empty<Vector4>(),
                Array.Empty<Vector2>(),
                Array.Empty<Color32>(),
                Array.Empty<int>(),
                new Bounds());

            var generated = new GeneratedChunkData(new WorldChunkId(0, 0), 0, mesh);

            Assert.That(generated.ResourcePlacements, Is.Empty);
            Assert.That(generated.SpawnPlacements, Is.Empty);
        }

        private static WorldGenerationRequest CreateRequest(WorldGenerationSeed seed, int lod, bool addSkirts)
        {
            return new WorldGenerationRequest(
                seed,
                WorldChunkBounds.Default,
                128f,
                64,
                lod,
                addSkirts,
                4f);
        }

        private static OpenWorldTerrainStreamingConfig CreateRuntimeConfig(string rootName)
        {
            var config = OpenWorldTerrainStreamingConfig.Default();
            config.RootName = rootName;
            config.ShowDebugGizmos = false;
            config.LogDebugStreaming = false;
            config.MaxChunkLoadsPerFrame = 256;
            return config;
        }

        private static bool AnyHeightDifferent(TerrainMeshData first, TerrainMeshData second)
        {
            for (var i = 0; i < first.Vertices.Length; i++)
            {
                if (Mathf.Abs(first.Vertices[i].y - second.Vertices[i].y) > 0.001f)
                    return true;
            }

            return false;
        }

        private static void AssertResourcePlacementsEqual(ResourcePlacement[] first, ResourcePlacement[] second)
        {
            Assert.That(second.Length, Is.EqualTo(first.Length));
            for (var i = 0; i < first.Length; i++)
                Assert.That(second[i], Is.EqualTo(first[i]));
        }

        private static void AssertSpawnPlacementsEqual(SpawnPlacement[] first, SpawnPlacement[] second)
        {
            Assert.That(second.Length, Is.EqualTo(first.Length));
            for (var i = 0; i < first.Length; i++)
                Assert.That(second[i], Is.EqualTo(first[i]));
        }

        private static bool ResourcePlacementsDiffer(ResourcePlacement[] first, ResourcePlacement[] second)
        {
            if (first.Length != second.Length)
                return true;

            for (var i = 0; i < first.Length; i++)
            {
                if (!first[i].Equals(second[i]))
                    return true;
            }

            return false;
        }

        private static bool SpawnPlacementsDiffer(SpawnPlacement[] first, SpawnPlacement[] second)
        {
            if (first.Length != second.Length)
                return true;

            for (var i = 0; i < first.Length; i++)
            {
                if (!first[i].Equals(second[i]))
                    return true;
            }

            return false;
        }

        private static void AssertPlacementsInsideChunk(ResourcePlacement[] placements, WorldChunkId chunkId, float chunkWorldSize)
        {
            var minX = chunkId.X * chunkWorldSize;
            var maxX = minX + chunkWorldSize;
            var minZ = chunkId.Z * chunkWorldSize;
            var maxZ = minZ + chunkWorldSize;
            for (var i = 0; i < placements.Length; i++)
                AssertPositionInsideChunk(placements[i].Position, minX, maxX, minZ, maxZ);
        }

        private static void AssertPlacementsInsideChunk(SpawnPlacement[] placements, WorldChunkId chunkId, float chunkWorldSize)
        {
            var minX = chunkId.X * chunkWorldSize;
            var maxX = minX + chunkWorldSize;
            var minZ = chunkId.Z * chunkWorldSize;
            var maxZ = minZ + chunkWorldSize;
            for (var i = 0; i < placements.Length; i++)
                AssertPositionInsideChunk(placements[i].Position, minX, maxX, minZ, maxZ);
        }

        private static void AssertPositionInsideChunk(Vector3 position, float minX, float maxX, float minZ, float maxZ)
        {
            Assert.That(position.x, Is.GreaterThanOrEqualTo(minX));
            Assert.That(position.x, Is.LessThanOrEqualTo(maxX));
            Assert.That(position.z, Is.GreaterThanOrEqualTo(minZ));
            Assert.That(position.z, Is.LessThanOrEqualTo(maxZ));
        }

        private static void AssertPlacementSamplesAreValid(ResourcePlacement[] placements, ISurfaceSampler sampler)
        {
            for (var i = 0; i < placements.Length; i++)
                AssertPlacementSampleIsValid(placements[i].Position, sampler);
        }

        private static void AssertPlacementSamplesAreValid(SpawnPlacement[] placements, ISurfaceSampler sampler)
        {
            for (var i = 0; i < placements.Length; i++)
                AssertPlacementSampleIsValid(placements[i].Position, sampler);
        }

        private static void AssertPlacementSampleIsValid(Vector3 position, ISurfaceSampler sampler)
        {
            var sample = sampler.Sample(position.x, position.z);
            Assert.That(sample.WaterMask, Is.LessThanOrEqualTo(OpenWorldPlacementGenerator.MAX_WATER_MASK));
            Assert.That(sample.Normal.y, Is.GreaterThanOrEqualTo(OpenWorldPlacementGenerator.MIN_NORMAL_Y));
            Assert.That(position.y, Is.EqualTo(sample.Height));
        }

        private static void AssertLayerProcGenNearBorderContinuity(
            WorldGenerationSeed seed,
            WorldChunkId chunkId,
            SeamAxis axis)
        {
            using var service = new LayerProcGenWorldGenerationService();
            var request = CreateRequest(seed, 0, false);
            var current = service.GenerateChunk(chunkId, request).TerrainMesh;
            var neighborId = axis == SeamAxis.PositiveX
                ? new WorldChunkId(chunkId.X + 1, chunkId.Z)
                : new WorldChunkId(chunkId.X, chunkId.Z + 1);
            var neighbor = service.GenerateChunk(neighborId, request).TerrainMesh;

            if (axis == SeamAxis.PositiveX)
                AssertPositiveXNearBorderContinuity(current, neighbor);
            else
                AssertPositiveZNearBorderContinuity(current, neighbor);
        }

        private static void AssertPositiveXNearBorderContinuity(TerrainMeshData left, TerrainMeshData right)
        {
            for (var z = 0; z <= LOD0_QUADS; z++)
            {
                var leftInteriorHeight = left.Vertices[GridIndex(LOD0_QUADS - 1, z, LOD0_QUADS)].y;
                var leftBorderHeight = left.Vertices[GridIndex(LOD0_QUADS, z, LOD0_QUADS)].y;
                var rightBorderHeight = right.Vertices[GridIndex(0, z, LOD0_QUADS)].y;
                var rightInteriorHeight = right.Vertices[GridIndex(1, z, LOD0_QUADS)].y;

                Assert.That(Mathf.Abs(leftBorderHeight - rightBorderHeight), Is.LessThan(0.0001f));
                Assert.That(Mathf.Abs(leftBorderHeight - leftInteriorHeight), Is.LessThan(MAX_NEAR_BORDER_HEIGHT_STEP));
                Assert.That(Mathf.Abs(rightInteriorHeight - rightBorderHeight), Is.LessThan(MAX_NEAR_BORDER_HEIGHT_STEP));
            }
        }

        private static void AssertPositiveZNearBorderContinuity(TerrainMeshData lower, TerrainMeshData upper)
        {
            for (var x = 0; x <= LOD0_QUADS; x++)
            {
                var lowerInteriorHeight = lower.Vertices[GridIndex(x, LOD0_QUADS - 1, LOD0_QUADS)].y;
                var lowerBorderHeight = lower.Vertices[GridIndex(x, LOD0_QUADS, LOD0_QUADS)].y;
                var upperBorderHeight = upper.Vertices[GridIndex(x, 0, LOD0_QUADS)].y;
                var upperInteriorHeight = upper.Vertices[GridIndex(x, 1, LOD0_QUADS)].y;

                Assert.That(Mathf.Abs(lowerBorderHeight - upperBorderHeight), Is.LessThan(0.0001f));
                Assert.That(Mathf.Abs(lowerBorderHeight - lowerInteriorHeight), Is.LessThan(MAX_NEAR_BORDER_HEIGHT_STEP));
                Assert.That(Mathf.Abs(upperInteriorHeight - upperBorderHeight), Is.LessThan(MAX_NEAR_BORDER_HEIGHT_STEP));
            }
        }

        private static void AssertLayerProcGenInteriorNormalContinuity(WorldGenerationSeed seed, WorldChunkId chunkId)
        {
            using var service = new LayerProcGenWorldGenerationService();
            var mesh = service.GenerateChunk(chunkId, CreateRequest(seed, 0, false)).TerrainMesh;

            for (var z = 0; z <= LOD0_QUADS; z++)
            {
                for (var x = 0; x <= LOD0_QUADS; x++)
                {
                    var current = mesh.Normals[GridIndex(x, z, LOD0_QUADS)];

                    if (x < LOD0_QUADS)
                    {
                        var right = mesh.Normals[GridIndex(x + 1, z, LOD0_QUADS)];
                        Assert.That(Vector3.Angle(current, right), Is.LessThan(MAX_INTERIOR_NORMAL_ANGLE));
                    }

                    if (z < LOD0_QUADS)
                    {
                        var up = mesh.Normals[GridIndex(x, z + 1, LOD0_QUADS)];
                        Assert.That(Vector3.Angle(current, up), Is.LessThan(MAX_INTERIOR_NORMAL_ANGLE));
                    }
                }
            }
        }

        private static int GridIndex(int x, int z, int quads)
        {
            return z * (quads + 1) + x;
        }

        private enum SeamAxis
        {
            PositiveX,
            PositiveZ
        }

        private sealed class FlatSurfaceSampler : ISurfaceSampler
        {
            public SurfaceSample Sample(float worldX, float worldZ)
            {
                return new SurfaceSample(0f, Vector3.up, 1, 1, 0f, 0f, 0f);
            }
        }
    }
}
