using System;
using System.Collections;
using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.AiNavigation;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Game;
using StaticMlp.Networking;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace StaticMlp.Tests.Ai
{
    public sealed class RuntimeNavMeshZoneTests
    {
        [Test]
        public void ChunkNavSourceRegistry_CollectSources_UsesStableChunkOrder()
        {
            using var registry = new ChunkNavSourceRegistry();
            var mesh = CreateFlatChunkMesh(10f);
            var bounds = new Bounds(new Vector3(10f, 0f, 10f), new Vector3(64f, 1000f, 64f));
            var sources = new List<NavMeshBuildSource>();

            registry.Register(new WorldChunkId(1, 0), 10f, mesh);
            registry.Register(new WorldChunkId(0, 1), 10f, mesh);
            registry.Register(new WorldChunkId(0, 0), 10f, mesh);
            registry.CollectSources(bounds, sources);

            Assert.That(sources.Count, Is.EqualTo(3));
            Assert.That(sources[0].transform.m03, Is.EqualTo(0f).Within(0.001f));
            Assert.That(sources[0].transform.m23, Is.EqualTo(0f).Within(0.001f));
            Assert.That(sources[1].transform.m03, Is.EqualTo(0f).Within(0.001f));
            Assert.That(sources[1].transform.m23, Is.EqualTo(10f).Within(0.001f));
            Assert.That(sources[2].transform.m03, Is.EqualTo(10f).Within(0.001f));
            Assert.That(sources[2].transform.m23, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void ChunkNavSourceRegistry_SourceSetVersion_IgnoresChangesOutsideCollectBounds()
        {
            using var registry = new ChunkNavSourceRegistry();
            var collectBounds = new Bounds(new Vector3(5f, 0f, 5f), new Vector3(12f, 1000f, 12f));

            registry.Register(new WorldChunkId(0, 0), 10f, CreateFlatChunkMesh(10f));
            var first = registry.CalculateSourceSetVersion(collectBounds, out var firstCount);

            registry.Register(new WorldChunkId(10, 10), 10f, CreateFlatChunkMesh(10f));
            var afterFarChunk = registry.CalculateSourceSetVersion(collectBounds, out var farCount);

            registry.Register(new WorldChunkId(0, 0), 10f, CreateRaisedChunkMesh(10f, 0.25f));
            var afterIntersectingChange = registry.CalculateSourceSetVersion(collectBounds, out var changedCount);

            Assert.That(firstCount, Is.EqualTo(1));
            Assert.That(farCount, Is.EqualTo(1));
            Assert.That(changedCount, Is.EqualTo(1));
            Assert.That(afterFarChunk, Is.EqualTo(first));
            Assert.That(afterIntersectingChange, Is.Not.EqualTo(first));
        }

        [Test]
        public void RuntimeNavMeshRebuildQueueSystem_TenChunkChanges_CoalesceIntoSingleZoneRequest()
        {
            using var scope = new AiNavigationTestServerWorldScope();
            var zone = scope.CreateZone(new float3(40f, 0f, 4f), sourceCollectRadius: 96f);
            var registry = SW.GetResource<ChunkNavSourceRegistry>();
            var system = new RuntimeNavMeshRebuildQueueSystem();

            for (var i = 0; i < 10; i++)
                registry.Register(new WorldChunkId(i, 0), 8f, CreateFlatChunkMesh(8f));

            system.Update();

            Assert.That(zone.Has<NavRebuildRequest>(), Is.True);
            Assert.That(zone.Read<RuntimeNavMeshZoneState>().RequestedNavVersion, Is.EqualTo(1));
            Assert.That(zone.Read<NavWorkBudgetCounter>().RebuildRequestsQueuedThisTick, Is.EqualTo(1));

            system.Update();

            Assert.That(zone.Read<RuntimeNavMeshZoneState>().RequestedNavVersion, Is.EqualTo(1));
            Assert.That(zone.Read<NavWorkBudgetCounter>().RebuildRequestsQueuedThisTick, Is.EqualTo(0));
        }

        [Test]
        public void RuntimeNavMeshRebuildQueueSystem_CombatCellCenterQuantization_AvoidsSmallMoveRebuild()
        {
            using var scope = new AiNavigationTestServerWorldScope();
            var zone = scope.CreateZone(new float3(1f, 0f, 1f), sourceCollectRadius: 32f);
            var registry = SW.GetResource<ChunkNavSourceRegistry>();
            var system = new RuntimeNavMeshRebuildQueueSystem();

            registry.Register(new WorldChunkId(0, 0), 16f, CreateFlatChunkMesh(16f));
            system.Update();

            zone.Mut<CombatCellNavArea>().Center = new float3(2f, 0f, 2f);
            system.Update();

            Assert.That(zone.Read<RuntimeNavMeshZoneState>().RequestedNavVersion, Is.EqualTo(1));
            Assert.That(zone.Read<NavWorkBudgetCounter>().RebuildRequestsQueuedThisTick, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator RuntimeNavMeshZoneBackend_SingleZoneAcrossAdjacentChunks_PathCompletes()
        {
            using var scope = new AiNavigationTestServerWorldScope();
            using var registry = new ChunkNavSourceRegistry();
            using var backend = new RuntimeNavMeshZoneBackend();
            var zone = SW.NewEntity<Default>();
            var origin = new Vector3(10000f, 0f, 10000f);
            var chunkSize = 10f;

            registry.Register(new WorldChunkId(1000, 1000), chunkSize, CreateFlatChunkMesh(chunkSize));
            registry.Register(new WorldChunkId(1001, 1000), chunkSize, CreateFlatChunkMesh(chunkSize));

            var sourceBounds = new Bounds(origin + new Vector3(10f, 0f, 5f), new Vector3(24f, 1000f, 14f));
            var sourceSetVersion = registry.CalculateSourceSetVersion(sourceBounds, out var sourceCount);
            backend.StartBuild(
                zone.GID,
                new RuntimeNavMeshZoneBackend.BuildInput(
                    sourceBounds,
                    sourceBounds,
                    1,
                    sourceSetVersion),
                registry);

            yield return WaitForBuild(backend, expectedBuilds: 1);

            var path = new NavMeshPath();
            var hasPath = NavMesh.CalculatePath(
                origin + new Vector3(2f, 0f, 5f),
                origin + new Vector3(18f, 0f, 5f),
                NavMesh.AllAreas,
                path);

            Assert.That(sourceCount, Is.EqualTo(2));
            Assert.That(hasPath, Is.True);
            Assert.That(path.status, Is.EqualTo(NavMeshPathStatus.PathComplete));
        }

        [UnityTest]
        public IEnumerator RuntimeNavMeshZoneBackend_OverlappingSeparateZonesWithoutLinks_DoNotCreateTransition()
        {
            using var scope = new AiNavigationTestServerWorldScope();
            using var registry = new ChunkNavSourceRegistry();
            using var backend = new RuntimeNavMeshZoneBackend();
            var firstZone = SW.NewEntity<Default>();
            var secondZone = SW.NewEntity<Default>();
            var chunkWorldSize = 8f;
            var origin = new Vector3(8000f, 0f, 8000f);

            registry.Register(new WorldChunkId(1000, 1000), chunkWorldSize, CreateFlatChunkMesh(10f));
            registry.Register(new WorldChunkId(1001, 1000), chunkWorldSize, CreateFlatChunkMesh(10f));

            var firstSourceBounds = new Bounds(origin + new Vector3(4f, 0f, 5f), new Vector3(7.8f, 1000f, 12f));
            var firstBuildBounds = new Bounds(origin + new Vector3(5f, 0f, 5f), new Vector3(10f, 1000f, 12f));
            var firstVersion = registry.CalculateSourceSetVersion(firstSourceBounds, out var firstSourceCount);
            backend.StartBuild(
                firstZone.GID,
                new RuntimeNavMeshZoneBackend.BuildInput(firstBuildBounds, firstSourceBounds, 1, firstVersion),
                registry);

            var secondSourceBounds = new Bounds(origin + new Vector3(14f, 0f, 5f), new Vector3(7.8f, 1000f, 12f));
            var secondBuildBounds = new Bounds(origin + new Vector3(13f, 0f, 5f), new Vector3(10f, 1000f, 12f));
            var secondVersion = registry.CalculateSourceSetVersion(secondSourceBounds, out var secondSourceCount);
            backend.StartBuild(
                secondZone.GID,
                new RuntimeNavMeshZoneBackend.BuildInput(secondBuildBounds, secondSourceBounds, 1, secondVersion),
                registry);

            yield return WaitForBuild(backend, expectedBuilds: 2);

            var path = new NavMeshPath();
            var hasPath = NavMesh.CalculatePath(
                origin + new Vector3(2f, 0f, 5f),
                origin + new Vector3(16f, 0f, 5f),
                NavMesh.AllAreas,
                path);

            Assert.That(firstSourceCount, Is.EqualTo(1));
            Assert.That(secondSourceCount, Is.EqualTo(1));
            Assert.That(hasPath && path.status == NavMeshPathStatus.PathComplete, Is.False);
        }

        private static IEnumerator WaitForBuild(RuntimeNavMeshZoneBackend backend, int expectedBuilds)
        {
            var results = new List<RuntimeNavMeshZoneBackend.BuildResult>();
            for (var frame = 0; frame < 120; frame++)
            {
                backend.CollectCompletedBuilds(results);
                if (results.Count >= expectedBuilds)
                    yield break;

                yield return null;
            }

            Assert.Fail("Runtime NavMesh build did not complete in 120 editor frames.");
        }

        private static TerrainMeshData CreateFlatChunkMesh(float size)
        {
            return CreateRaisedChunkMesh(size, 0f);
        }

        private static TerrainMeshData CreateRaisedChunkMesh(float size, float centerHeight)
        {
            var vertices = new[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(size, centerHeight, 0f),
                new Vector3(0f, 0f, size),
                new Vector3(size, 0f, size)
            };
            var triangles = new[] { 0, 2, 1, 1, 2, 3 };
            var bounds = new Bounds(
                new Vector3(size * 0.5f, centerHeight * 0.5f, size * 0.5f),
                new Vector3(size, Math.Max(1f, centerHeight), size));

            return new TerrainMeshData(
                vertices,
                null,
                null,
                null,
                null,
                triangles,
                bounds);
        }

        private sealed class AiNavigationTestServerWorldScope : IDisposable
        {
            public AiNavigationTestServerWorldScope()
            {
                if (SW.Status != WorldStatus.NotCreated)
                    SW.Destroy();

                SW.Create(WorldConfig.Default());
                SW.Types().RegisterAll(
                    typeof(ServerWT).Assembly,
                    typeof(CombatCellNavArea).Assembly);
                SW.Initialize();
                SW.SetResource(new SimulationTime
                {
                    FixedStepSeconds = 0.5f
                });
                SW.SetResource(new ChunkNavSourceRegistry());
                SW.SetResource(new RuntimeNavMeshZoneBackend());
            }

            public SW.Entity CreateZone(float3 center, float sourceCollectRadius)
            {
                var zone = SW.NewEntity<Default>();
                zone.Set(new CombatCellNavArea
                {
                    CellId = 1,
                    Center = center,
                    Radius = 12f,
                    NavBuildRadius = 24f,
                    SourceCollectRadius = sourceCollectRadius,
                    Priority = 100
                });
                zone.Set(default(RuntimeNavMeshZoneState));
                zone.Set(new CombatCellPerformanceBudget
                {
                    MaxNavRebuildStartsPerTick = 1
                });
                zone.Set(default(NavWorkBudgetCounter));
                return zone;
            }

            public void Dispose()
            {
                if (SW.Status != WorldStatus.NotCreated)
                {
                    SW.GetResource<RuntimeNavMeshZoneBackend>().Dispose();
                    SW.GetResource<ChunkNavSourceRegistry>().Dispose();
                    SW.Destroy();
                }
            }
        }
    }
}
