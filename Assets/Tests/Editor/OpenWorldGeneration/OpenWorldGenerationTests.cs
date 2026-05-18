using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.OpenWorldGeneration.Jobs;
using NUnit.Framework;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Game.Bootstrap;
using StaticMlp.LayerProcLite;
using StaticMlp.Networking;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
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
        public void FromWorldPosition_UsesFloorForNegativeCoordinates()
        {
            Assert.That(WorldChunkId.FromWorldPosition(-0.01f, -0.01f, 128f), Is.EqualTo(new WorldChunkId(-1, -1)));
            Assert.That(WorldChunkId.FromWorldPosition(-128f, -128.01f, 128f), Is.EqualTo(new WorldChunkId(-1, -2)));
            Assert.That(WorldChunkId.FromWorldPosition(0f, 127.99f, 128f), Is.EqualTo(new WorldChunkId(0, 0)));
        }

        [Test]
        public void SpatialClusterIds_MapChunksRowMajorInsideBounds()
        {
            var bounds = new WorldChunkBounds(-1, 1, -2, -1);

            Assert.That(OpenWorldSpatialClusterIds.ToClusterId(new WorldChunkId(-1, -2), bounds), Is.EqualTo(OpenWorldSpatialClusterIds.FIRST_CLUSTER_ID));
            Assert.That(OpenWorldSpatialClusterIds.ToClusterId(new WorldChunkId(0, -2), bounds), Is.EqualTo(OpenWorldSpatialClusterIds.FIRST_CLUSTER_ID + 1));
            Assert.That(OpenWorldSpatialClusterIds.ToClusterId(new WorldChunkId(1, -1), bounds), Is.EqualTo(OpenWorldSpatialClusterIds.FIRST_CLUSTER_ID + 5));
        }

        [Test]
        public void SpatialClusterIds_RoundTripClusterAndChunk()
        {
            var bounds = new WorldChunkBounds(-3, 2, -4, 1);
            var chunkId = new WorldChunkId(2, -1);
            var clusterId = OpenWorldSpatialClusterIds.ToClusterId(chunkId, bounds);

            Assert.That(OpenWorldSpatialClusterIds.ToChunkId(clusterId, bounds), Is.EqualTo(chunkId));
        }

        [Test]
        public void SpatialClusterIds_WhenChunkOutsideBounds_Throws()
        {
            var bounds = new WorldChunkBounds(0, 1, 0, 1);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                OpenWorldSpatialClusterIds.ToClusterId(new WorldChunkId(2, 0), bounds));
        }

        [Test]
        public void SpatialClusterIds_WhenBoundsExceedUshortCapacity_Throws()
        {
            var bounds = new WorldChunkBounds(0, ushort.MaxValue, 0, 0);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                OpenWorldSpatialClusterIds.ToClusterId(new WorldChunkId(0, 0), bounds));
        }

        [Test]
        public void OpenWorldGenerationRequestKey_DistinguishesLodAndLayerMask()
        {
            var chunkId = new LayerProcLiteChunkId(2, -3);
            var meshLayers = LayerProcLiteLayerMask.From(OpenWorldGenerationLayerIds.MeshData);
            var placementLayers = LayerProcLiteLayerMask.From(OpenWorldGenerationLayerIds.Placements);
            var mesh = new OpenWorldGenerationRequestKey(chunkId, 0, meshLayers, GenerationOutputMask.VisualMesh, 99);
            var placements = new OpenWorldGenerationRequestKey(chunkId, 0, placementLayers, GenerationOutputMask.Placements, 99);
            var lod = new OpenWorldGenerationRequestKey(chunkId, 1, meshLayers, GenerationOutputMask.VisualMesh, 99);
            var physics = new OpenWorldGenerationRequestKey(chunkId, 0, meshLayers, GenerationOutputMask.PhysicsMesh, 99);

            Assert.That(mesh, Is.Not.EqualTo(placements));
            Assert.That(mesh, Is.Not.EqualTo(lod));
            Assert.That(mesh, Is.Not.EqualTo(physics));
        }

        [Test]
        public void OpenWorldGenerationFeature_OnHost_ReusesServerGenerationRuntimeForClient()
        {
            DestroyOpenWorldTestWorlds();
            OpenWorldChunkGenerationRuntime serverRuntime = null;
            OpenWorldChunkGenerationRuntime clientRuntime = null;

            try
            {
                RegisterHostOpenWorldFeature(out serverRuntime, out clientRuntime);

                Assert.That(clientRuntime, Is.SameAs(serverRuntime));
            }
            finally
            {
                DisposeDistinctRuntimes(serverRuntime, clientRuntime);
                DestroyOpenWorldTestWorlds();
            }
        }

        [Test]
        public void OpenWorldGenerationFeature_OnClientOnly_CreatesClientGenerationRuntime()
        {
            DestroyOpenWorldTestWorlds();
            OpenWorldChunkGenerationRuntime clientRuntime = null;

            try
            {
                CreateOpenWorldClientWorld();
                RegisterClientOpenWorldFeature(new OpenWorldGenerationGameplayFeature());
                clientRuntime = CW.GetResource<OpenWorldChunkGenerationRuntime>();

                Assert.That(SW.Status, Is.EqualTo(WorldStatus.NotCreated));
                Assert.That(clientRuntime, Is.Not.Null);
                Assert.That(clientRuntime.LayerRuntime, Is.Not.Null);
            }
            finally
            {
                clientRuntime?.Dispose();
                DestroyOpenWorldTestWorlds();
            }
        }

        [Test]
        public void HostSharedGenerationRuntime_WhenServerAndClientRequestSameChunk_ReusesLayerRuntimeChunkGraph()
        {
            DestroyOpenWorldTestWorlds();
            OpenWorldChunkGenerationRuntime serverRuntime = null;
            OpenWorldChunkGenerationRuntime clientRuntime = null;
            var serverDependency = default(LayerProcLiteTopDependencyId);
            var clientDependency = default(LayerProcLiteTopDependencyId);
            var hasServerDependency = false;
            var hasClientDependency = false;

            try
            {
                RegisterHostOpenWorldFeature(out serverRuntime, out clientRuntime);
                var request = CreateRequest(new WorldGenerationSeed(12345), 0, true);
                var chunkId = new WorldChunkId(0, 0);

                serverDependency = AddVisualMeshTopDependency(serverRuntime, chunkId, request);
                hasServerDependency = true;
                var chunksAfterServerRequest = serverRuntime.LayerRuntime.ChunkCount;

                clientDependency = AddVisualMeshTopDependency(clientRuntime, chunkId, request);
                hasClientDependency = true;

                Assert.That(clientRuntime.LayerRuntime, Is.SameAs(serverRuntime.LayerRuntime));
                Assert.That(chunksAfterServerRequest, Is.GreaterThan(0));
                Assert.That(clientRuntime.LayerRuntime.ChunkCount, Is.EqualTo(chunksAfterServerRequest));
                Assert.That(clientRuntime.LayerRuntime.ActiveTopDependencyCount, Is.EqualTo(2));
            }
            finally
            {
                if (hasClientDependency)
                    clientRuntime.LayerRuntime.RemoveTopDependency(clientDependency);
                if (hasServerDependency)
                    serverRuntime.LayerRuntime.RemoveTopDependency(serverDependency);

                DisposeDistinctRuntimes(serverRuntime, clientRuntime);
                DestroyOpenWorldTestWorlds();
            }
        }

        [Test]
        public void LayerProcLite_GridLayout_AddsInputPadding()
        {
            var layout = new LayerProcLiteGridLayout(17, 1);

            Assert.That(layout.OutputSampleCount, Is.EqualTo(17 * 17));
            Assert.That(layout.InputSampleCount, Is.EqualTo(19 * 19));
            Assert.That(layout.ToInputIndex(0, 0), Is.EqualTo(20));
        }

        [Test]
        public void LayerProcLite_DeterministicHash_IsStable()
        {
            var chunkId = new LayerProcLiteChunkId(-2, 5);

            var first = LayerProcLiteDeterministicHash.Hash(123u, chunkId, 11, 7);
            var second = LayerProcLiteDeterministicHash.Hash(123u, chunkId, 11, 7);

            Assert.That(second, Is.EqualTo(first));
            Assert.That(LayerProcLiteDeterministicHash.Unit(first), Is.InRange(0f, 1f));
        }

        [Test]
        public void LayerProcLiteNativeGrid_UsesPaddedLayout()
        {
            var layout = new LayerProcLiteGridLayout(3, 1);
            var values = new NativeArray<int>(layout.InputSampleCount, Allocator.Temp);

            try
            {
                var grid = new LayerProcLiteNativeGrid<int>(values, layout);

                grid.SetOutput(0, 0, 42);

                Assert.That(values[layout.ToInputIndex(0, 0)], Is.EqualTo(42));
            }
            finally
            {
                values.Dispose();
            }
        }

        [Test]
        public void LayerProcLiteRuntime_ConvertsNegativeBoundsToProviderChunkKeys()
        {
            var layer = new LayerProcLiteLayerId(0);
            var scheduler = new CountingLayerScheduler();
            using var runtime = new LayerProcLiteRuntime()
                .RegisterLayer(new LayerProcLiteLayerDefinition(layer, 10f, scheduler));

            var id = runtime.AddTopDependency(new LayerProcLiteTopDependencyRequest(
                layer,
                0,
                new LayerProcLiteWorldBounds(-10.1f, -0.1f, 0.1f, 10.1f)));

            Assert.That(scheduler.ScheduleCount, Is.EqualTo(9));
            Assert.That(runtime.ContainsChunk(new LayerProcLiteChunkKey(layer, 0, new LayerProcLiteChunkId(-2, -1))), Is.True);
            Assert.That(runtime.ContainsChunk(new LayerProcLiteChunkKey(layer, 0, new LayerProcLiteChunkId(0, 1))), Is.True);

            runtime.RemoveTopDependency(id);
            Assert.That(runtime.ChunkCount, Is.EqualTo(0));
        }

        [Test]
        public void LayerProcLiteRuntime_OverlappingTopDependenciesDedupeAndRelease()
        {
            var layer = new LayerProcLiteLayerId(0);
            var scheduler = new CountingLayerScheduler();
            using var runtime = new LayerProcLiteRuntime()
                .RegisterLayer(new LayerProcLiteLayerDefinition(layer, 10f, scheduler));
            var request = new LayerProcLiteTopDependencyRequest(
                layer,
                0,
                new LayerProcLiteWorldBounds(0f, 0f, 10f, 10f));
            var key = new LayerProcLiteChunkKey(layer, 0, new LayerProcLiteChunkId(0, 0));

            var first = runtime.AddTopDependency(request);
            var second = runtime.AddTopDependency(request);

            Assert.That(scheduler.ScheduleCount, Is.EqualTo(1));
            Assert.That(runtime.GetRetainCount(key), Is.EqualTo(2));

            runtime.RemoveTopDependency(first);
            Assert.That(runtime.ContainsChunk(key), Is.True);
            Assert.That(runtime.GetRetainCount(key), Is.EqualTo(1));

            runtime.RemoveTopDependency(second);
            Assert.That(runtime.ContainsChunk(key), Is.False);
            Assert.That(scheduler.DisposeCount, Is.EqualTo(1));
        }

        [Test]
        public void LayerProcLiteRuntime_MovingTopDependencyReleasesOldChunks()
        {
            var layer = new LayerProcLiteLayerId(0);
            var scheduler = new CountingLayerScheduler();
            using var runtime = new LayerProcLiteRuntime()
                .RegisterLayer(new LayerProcLiteLayerDefinition(layer, 10f, scheduler));
            var oldKey = new LayerProcLiteChunkKey(layer, 0, new LayerProcLiteChunkId(0, 0));
            var newKey = new LayerProcLiteChunkKey(layer, 0, new LayerProcLiteChunkId(1, 0));
            var id = runtime.AddTopDependency(new LayerProcLiteTopDependencyRequest(
                layer,
                0,
                new LayerProcLiteWorldBounds(0f, 0f, 10f, 10f)));

            runtime.SetTopDependency(id, new LayerProcLiteTopDependencyRequest(
                layer,
                0,
                new LayerProcLiteWorldBounds(10f, 0f, 20f, 10f)));

            Assert.That(runtime.ContainsChunk(oldKey), Is.False);
            Assert.That(runtime.ContainsChunk(newKey), Is.True);
            Assert.That(runtime.GetRetainCount(newKey), Is.EqualTo(1));
            Assert.That(scheduler.ScheduleCount, Is.EqualTo(2));
            Assert.That(scheduler.DisposeCount, Is.EqualTo(1));
        }

        [Test]
        public void LayerProcLiteRuntime_ExpandsProviderBoundsFromEffectDistance()
        {
            var provider = new LayerProcLiteLayerId(0);
            var consumer = new LayerProcLiteLayerId(1);
            var providerScheduler = new CountingLayerScheduler();
            var consumerScheduler = new CountingLayerScheduler();
            using var runtime = new LayerProcLiteRuntime()
                .RegisterLayer(new LayerProcLiteLayerDefinition(provider, 10f, providerScheduler))
                .RegisterLayer(new LayerProcLiteLayerDefinition(
                    consumer,
                    10f,
                    consumerScheduler,
                    new LayerProcLiteDependency(provider, 0, 0, 0, 10f)));

            runtime.AddTopDependency(new LayerProcLiteTopDependencyRequest(
                consumer,
                0,
                new LayerProcLiteWorldBounds(0f, 0f, 10f, 10f)));

            Assert.That(providerScheduler.ScheduleCount, Is.EqualTo(9));
            Assert.That(consumerScheduler.ScheduleCount, Is.EqualTo(1));
        }

        [Test]
        public void LayerProcLiteRuntime_DetectsLayerLevelCycles()
        {
            var a = new LayerProcLiteLayerId(0);
            var b = new LayerProcLiteLayerId(1);
            using var runtime = new LayerProcLiteRuntime()
                .RegisterLayer(new LayerProcLiteLayerDefinition(a, 10f, new CountingLayerScheduler(), new LayerProcLiteDependency(b, 0, 0f)))
                .RegisterLayer(new LayerProcLiteLayerDefinition(b, 10f, new CountingLayerScheduler(), new LayerProcLiteDependency(a, 0, 0f)));

            Assert.Throws<InvalidOperationException>(() =>
                runtime.AddTopDependency(new LayerProcLiteTopDependencyRequest(
                    a,
                    0,
                    new LayerProcLiteWorldBounds(0f, 0f, 10f, 10f))));
        }

        [Test]
        public void LayerProcLiteRuntime_AllowsSpecificProviderLevelDependency()
        {
            var layer = new LayerProcLiteLayerId(0);
            var scheduler = new CountingLayerScheduler();
            using var runtime = new LayerProcLiteRuntime()
                .RegisterLayer(new LayerProcLiteLayerDefinition(
                    layer,
                    10f,
                    2,
                    scheduler,
                    new LayerProcLiteDependency(layer, 1, 0, 0, 0f)));

            runtime.AddTopDependency(new LayerProcLiteTopDependencyRequest(
                layer,
                1,
                new LayerProcLiteWorldBounds(0f, 0f, 10f, 10f)));

            Assert.That(runtime.ContainsChunk(new LayerProcLiteChunkKey(layer, 0, new LayerProcLiteChunkId(0, 0))), Is.True);
            Assert.That(runtime.ContainsChunk(new LayerProcLiteChunkKey(layer, 1, new LayerProcLiteChunkId(0, 0))), Is.True);
            Assert.That(scheduler.ScheduleCount, Is.EqualTo(2));
        }

        [Test]
        public void LayerProcLiteProviderSet_WhenAccessExceedsDeclaredBounds_Throws()
        {
            var provider = new LayerProcLiteLayerId(0);
            var consumer = new LayerProcLiteLayerId(1);
            using var runtime = new LayerProcLiteRuntime()
                .RegisterLayer(new LayerProcLiteLayerDefinition(provider, 10f, new CountingLayerScheduler()))
                .RegisterLayer(new LayerProcLiteLayerDefinition(
                    consumer,
                    10f,
                    new OutOfBoundsProviderScheduler(provider),
                    new LayerProcLiteDependency(provider, 0, 0, 0, 1f)));

            Assert.Throws<InvalidOperationException>(() =>
                runtime.AddTopDependency(new LayerProcLiteTopDependencyRequest(
                    consumer,
                    0,
                    new LayerProcLiteWorldBounds(0f, 0f, 10f, 10f))));
        }

        [Test]
        public void OpenWorldLayerCatalog_MapsMeshOutputsToSingleMeshDataLayer()
        {
            var layerIds = OpenWorldGenerationLayerCatalog.ToOutputLayerIds(
                GenerationOutputMask.VisualMesh
                | GenerationOutputMask.PhysicsMesh
                | GenerationOutputMask.NavMeshSourceMesh
                | GenerationOutputMask.Placements);
            var layers = OpenWorldGenerationLayerCatalog.ToLayerMask(
                GenerationOutputMask.VisualMesh
                | GenerationOutputMask.PhysicsMesh
                | GenerationOutputMask.NavMeshSourceMesh
                | GenerationOutputMask.Placements);

            Assert.That(layerIds, Is.EqualTo(new[] { OpenWorldGenerationLayerIds.MeshData, OpenWorldGenerationLayerIds.Placements }));
            Assert.That(layers.Contains(OpenWorldGenerationLayerIds.MeshData), Is.True);
            Assert.That(layers.Contains(OpenWorldGenerationLayerIds.Placements), Is.True);
        }

        [Test]
        public void OpenWorldLayerCatalog_SurfaceTopDependencySchedulesPaddedHeightProvider()
        {
            using var config = OpenWorldChunkGenerationRuntime.CreateDefault();
            var runtime = config.LayerRuntime;
            var settings = new OpenWorldLayerGenerationSettings(
                (uint)config.Seed.Value,
                config.ChunkWorldSize,
                config.WaterLevel,
                config.BaseQuadCount,
                0,
                config.AddSkirts,
                config.SkirtDepth);
            var id = runtime.AddTopDependency(new LayerProcLiteTopDependencyRequest(
                OpenWorldGenerationLayerIds.Surface,
                0,
                new LayerProcLiteWorldBounds(0f, 0f, 128f, 128f),
                0,
                0,
                settings));
            var heightKey = new LayerProcLiteChunkKey(
                OpenWorldGenerationLayerIds.Height,
                0,
                new LayerProcLiteChunkId(0, 0));

            Assert.That(runtime.ContainsChunk(heightKey), Is.True);
            Assert.That(runtime.GetRetainCount(heightKey), Is.EqualTo(1));

            runtime.RemoveTopDependency(id);
        }

        [Test]
        public void OpenWorldLayerCatalog_MeshDataTopDependenciesShareGeneratedChunk()
        {
            using var config = OpenWorldChunkGenerationRuntime.CreateDefault();
            var runtime = config.LayerRuntime;
            var settings = new OpenWorldLayerGenerationSettings(
                (uint)config.Seed.Value,
                config.ChunkWorldSize,
                config.WaterLevel,
                config.BaseQuadCount,
                0,
                config.AddSkirts,
                config.SkirtDepth);
            var bounds = new LayerProcLiteWorldBounds(0f, 0f, 128f, 128f);
            var first = runtime.AddTopDependency(new LayerProcLiteTopDependencyRequest(
                OpenWorldGenerationLayerIds.MeshData,
                0,
                bounds,
                0,
                0,
                settings));
            var second = runtime.AddTopDependency(new LayerProcLiteTopDependencyRequest(
                OpenWorldGenerationLayerIds.MeshData,
                0,
                bounds,
                0,
                0,
                settings));
            var meshDataKey = new LayerProcLiteChunkKey(
                OpenWorldGenerationLayerIds.MeshData,
                0,
                new LayerProcLiteChunkId(0, 0));

            Assert.That(runtime.ContainsChunk(meshDataKey), Is.True);
            Assert.That(runtime.GetRetainCount(meshDataKey), Is.EqualTo(2));

            runtime.RemoveTopDependency(first);
            Assert.That(runtime.GetRetainCount(meshDataKey), Is.EqualTo(1));

            runtime.RemoveTopDependency(second);
            Assert.That(runtime.ContainsChunk(meshDataKey), Is.False);
        }

        [Test]
        public void OpenWorldJobs_SurfaceNormals_AreContinuousAcrossChunkSeam()
        {
            const int resolution = 33;
            const float chunkWorldSize = 128f;
            var heightStep = new LayerProcLitePlanStep(
                OpenWorldGenerationLayerIds.Height,
                LayerProcLiteLayerMask.None,
                new LayerProcLiteWindow(OpenWorldGenerationLayerCatalog.SURFACE_HEIGHT_PADDING_SAMPLES, 0f));
            var surfaceStep = new LayerProcLitePlanStep(
                OpenWorldGenerationLayerIds.Surface,
                LayerProcLiteLayerMask.From(OpenWorldGenerationLayerIds.Height),
                LayerProcLiteWindow.None);
            var layout = new LayerProcLiteGridLayout(resolution, heightStep.Window.PaddingSamples);
            var leftHeights = new NativeArray<float>(layout.InputSampleCount, Allocator.TempJob);
            var rightHeights = new NativeArray<float>(layout.InputSampleCount, Allocator.TempJob);
            var leftSurfaces = new NativeArray<OpenWorldNativeSurfaceSample>(layout.OutputSampleCount, Allocator.TempJob);
            var rightSurfaces = new NativeArray<OpenWorldNativeSurfaceSample>(layout.OutputSampleCount, Allocator.TempJob);

            try
            {
                var leftHeightJob = new OpenWorldHeightmapGenerationJob
                {
                    Step = heightStep,
                    ChunkId = new LayerProcLiteChunkId(0, 0),
                    WorldSeed = 42,
                    ChunkWorldSize = chunkWorldSize,
                    OutputResolution = resolution,
                    PaddedHeights = leftHeights
                }.Schedule(layout.InputSampleCount, 16);
                var rightHeightJob = new OpenWorldHeightmapGenerationJob
                {
                    Step = heightStep,
                    ChunkId = new LayerProcLiteChunkId(1, 0),
                    WorldSeed = 42,
                    ChunkWorldSize = chunkWorldSize,
                    OutputResolution = resolution,
                    PaddedHeights = rightHeights
                }.Schedule(layout.InputSampleCount, 16);
                var leftSurfaceJob = new OpenWorldSurfaceSamplingJob
                {
                    Step = surfaceStep,
                    ChunkId = new LayerProcLiteChunkId(0, 0),
                    WorldSeed = 42,
                    ChunkWorldSize = chunkWorldSize,
                    OutputResolution = resolution,
                    HeightLayout = layout,
                    WaterLevel = -7f,
                    PaddedHeights = leftHeights.AsReadOnly(),
                    Surfaces = leftSurfaces
                }.Schedule(layout.OutputSampleCount, 16, leftHeightJob);
                var rightSurfaceJob = new OpenWorldSurfaceSamplingJob
                {
                    Step = surfaceStep,
                    ChunkId = new LayerProcLiteChunkId(1, 0),
                    WorldSeed = 42,
                    ChunkWorldSize = chunkWorldSize,
                    OutputResolution = resolution,
                    HeightLayout = layout,
                    WaterLevel = -7f,
                    PaddedHeights = rightHeights.AsReadOnly(),
                    Surfaces = rightSurfaces
                }.Schedule(layout.OutputSampleCount, 16, rightHeightJob);

                JobHandle.CombineDependencies(leftSurfaceJob, rightSurfaceJob).Complete();

                for (var z = 0; z < resolution; z++)
                {
                    var left = leftSurfaces[LayerProcLiteGrid.ToIndex(resolution - 1, z, resolution)].Normal;
                    var right = rightSurfaces[LayerProcLiteGrid.ToIndex(0, z, resolution)].Normal;
                    Assert.That(math.distance(left, right), Is.LessThan(0.0001f));
                }
            }
            finally
            {
                leftHeights.Dispose();
                rightHeights.Dispose();
                leftSurfaces.Dispose();
                rightSurfaces.Dispose();
            }
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
            DestroyOpenWorldTestWorlds();
            var config = CreateRuntimeConfig("OpenWorldTerrainDebugSnapshotTest");
            config.Bounds = new WorldChunkBounds(-1, 1, -1, 1);
            config.ViewRadiusInChunks = 1;
            config.ColliderRadiusInChunks = 0;
            var runtime = OpenWorldTerrainRuntime.Create(config);

            try
            {
                CreateOpenWorldClientWorld();
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
                DestroyOpenWorldTestWorlds();
            }
        }

        [Test]
        public void TerrainRuntime_Dispose_DestroysRootChunksAndMeshesInEditMode()
        {
            DestroyOpenWorldTestWorlds();
            const string rootName = "OpenWorldTerrainCleanupTest";
            var config = CreateRuntimeConfig(rootName);
            config.Bounds = new WorldChunkBounds(0, 0, 0, 0);
            config.ViewRadiusInChunks = 0;
            var runtime = OpenWorldTerrainRuntime.Create(config);
            var disposed = false;

            try
            {
                CreateOpenWorldClientWorld();
                runtime.StreamAround(Vector3.zero);

                Assert.That(GameObject.Find(rootName), Is.Not.Null);
                Assert.That(UnityEngine.Object.FindObjectsOfType<TerrainChunkView>(), Is.Not.Empty);

                runtime.Dispose();
                disposed = true;

                Assert.That(GameObject.Find(rootName), Is.Null);
                Assert.That(UnityEngine.Object.FindObjectsOfType<TerrainChunkView>(), Is.Empty);
            }
            finally
            {
                if (!disposed)
                    runtime.Dispose();
                DestroyOpenWorldTestWorlds();
            }
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

        private static void RegisterHostOpenWorldFeature(
            out OpenWorldChunkGenerationRuntime serverRuntime,
            out OpenWorldChunkGenerationRuntime clientRuntime)
        {
            var feature = new OpenWorldGenerationGameplayFeature();
            CreateOpenWorldServerWorld();
            feature.RegisterServerResources();
            serverRuntime = SW.GetResource<OpenWorldChunkGenerationRuntime>();

            CreateOpenWorldClientWorld();
            RegisterClientOpenWorldFeature(feature);
            clientRuntime = CW.GetResource<OpenWorldChunkGenerationRuntime>();
        }

        private static void RegisterClientOpenWorldFeature(OpenWorldGenerationGameplayFeature feature)
        {
            ClientCoreSys.Create();
            try
            {
                feature.RegisterClientCoreSystems(new ClientCoreSystemsBuilder());
            }
            finally
            {
                ClientCoreSys.Destroy();
            }
        }

        private static void CreateOpenWorldServerWorld()
        {
            SW.Create(WorldConfig.Default());
            SW.Types().RegisterAll(
                typeof(ServerWT).Assembly,
                typeof(OpenWorldGenerationGameplayFeature).Assembly,
                typeof(OpenWorldChunkGenerationRequested).Assembly);
            SW.Initialize();
        }

        private static void CreateOpenWorldClientWorld()
        {
            CW.Create(WorldConfig.Default());
            CW.Types().RegisterAll(
                typeof(ClientCoreWT).Assembly,
                typeof(OpenWorldGenerationGameplayFeature).Assembly,
                typeof(OpenWorldChunkGenerationRequested).Assembly);
            CW.Initialize();
        }

        private static LayerProcLiteTopDependencyId AddVisualMeshTopDependency(
            OpenWorldChunkGenerationRuntime runtime,
            WorldChunkId chunkId,
            WorldGenerationRequest request)
        {
            var minX = chunkId.X * request.ChunkWorldSize;
            var minZ = chunkId.Z * request.ChunkWorldSize;
            var settings = new OpenWorldLayerGenerationSettings(
                (uint)request.Seed.Value,
                request.ChunkWorldSize,
                runtime.WaterLevel,
                request.BaseQuadCount,
                request.Lod,
                request.AddSkirts,
                request.SkirtDepth);

            return runtime.LayerRuntime.AddTopDependency(
                new LayerProcLiteTopDependencyRequest(
                    OpenWorldGenerationLayerIds.MeshData,
                    0,
                    new LayerProcLiteWorldBounds(
                        minX,
                        minZ,
                        minX + request.ChunkWorldSize,
                        minZ + request.ChunkWorldSize),
                    request.Lod,
                    0,
                    settings));
        }

        private static void DisposeDistinctRuntimes(
            OpenWorldChunkGenerationRuntime first,
            OpenWorldChunkGenerationRuntime second)
        {
            if (ReferenceEquals(first, second))
            {
                first?.Dispose();
                return;
            }

            first?.Dispose();
            second?.Dispose();
        }

        private static void DestroyOpenWorldTestWorlds()
        {
            if (CW.Status != WorldStatus.NotCreated)
                CW.Destroy();
            if (SW.Status != WorldStatus.NotCreated)
                SW.Destroy();
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

        private static int GridIndex(int x, int z, int quads)
        {
            return z * (quads + 1) + x;
        }

        private enum SeamAxis
        {
            PositiveX,
            PositiveZ
        }

        private sealed class CountingLayerScheduler : ILayerProcLiteLayerScheduler
        {
            public int ScheduleCount;
            public int DisposeCount;

            public LayerProcLiteScheduleResult Schedule(in LayerProcLiteScheduleContext context)
            {
                ScheduleCount++;
                return new LayerProcLiteScheduleResult(default, new CountingChunkData(this));
            }
        }

        private sealed class OutOfBoundsProviderScheduler : ILayerProcLiteLayerScheduler
        {
            private readonly LayerProcLiteLayerId _providerLayerId;

            public OutOfBoundsProviderScheduler(LayerProcLiteLayerId providerLayerId)
            {
                _providerLayerId = providerLayerId;
            }

            public LayerProcLiteScheduleResult Schedule(in LayerProcLiteScheduleContext context)
            {
                context.Providers.GetOverlapping<CountingChunkData>(
                    _providerLayerId,
                    0,
                    context.Bounds.Expanded(2f));

                return new LayerProcLiteScheduleResult(default, new CountingChunkData(null));
            }
        }

        private sealed class CountingChunkData : ILayerProcLiteChunkData
        {
            private readonly CountingLayerScheduler _scheduler;

            public CountingChunkData(CountingLayerScheduler scheduler)
            {
                _scheduler = scheduler;
            }

            public void Dispose()
            {
                if (_scheduler != null)
                    _scheduler.DisposeCount++;
            }
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
