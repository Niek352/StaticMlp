using System;
using System.IO;
using System.Linq;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Features.OpenWorldResources;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Tests.OpenWorldResources
{
    public sealed class OpenWorldResourcesTests
    {
        [Test]
        public void ServerSeedSystem_CreatesAuthoritativeResourceNodesFromGeneratedPlacements()
        {
            using var scope = new OpenWorldResourcesServerWorldScope();
            var expected = scope.GenerateRequestedChunk().ResourcePlacements.Length;

            scope.RunSeedSystem();

            Assert.That(expected, Is.GreaterThan(0));
            Assert.That(CountResourceNodes(), Is.EqualTo(expected));
            var authoritativeCount = 0;
            foreach (var entity in SW.Query<All<OpenWorldResourceNodeTag, OpenWorldResourceNodeState, OpenWorldResourceNodeTransform, ServerOwned>>().Entities())
            {
                authoritativeCount++;
                ref readonly var identity = ref entity.Read<NetworkIdentity>();
                Assert.That(identity.Authority, Is.EqualTo(NetworkAuthority.Server));
                Assert.That(identity.NetworkArchetypeId, Is.EqualTo(OpenWorldResourceNetworkArchetypeIds.ResourceNode));
            }
            Assert.That(authoritativeCount, Is.EqualTo(expected));
        }

        [Test]
        public void ServerSeedSystem_SpawnedResourceNodesIncludeChunkRef()
        {
            using var scope = new OpenWorldResourcesServerWorldScope();

            scope.RunSeedSystem();

            var count = 0;
            foreach (var entity in SW.Query<All<OpenWorldResourceNodeTag, OpenWorldChunkRef>>().Entities())
            {
                count++;
                ref readonly var chunkRef = ref entity.Read<OpenWorldChunkRef>();
                Assert.That(chunkRef.X, Is.EqualTo(0));
                Assert.That(chunkRef.Z, Is.EqualTo(0));
            }

            Assert.That(count, Is.GreaterThan(0));
        }

        [Test]
        public void ServerSeedSystem_WhenRerun_DoesNotDuplicatePlacements()
        {
            using var scope = new OpenWorldResourcesServerWorldScope();

            scope.RunSeedSystem();
            var firstCount = CountResourceNodes();
            scope.RunSeedSystem();

            Assert.That(CountResourceNodes(), Is.EqualTo(firstCount));
        }

        [Test]
        public void ServerSeedSystem_DepletedPlacementIds_AreNotRespawned()
        {
            using var scope = new OpenWorldResourcesServerWorldScope();
            var depletedPlacementId = scope.GenerateRequestedChunk().ResourcePlacements[0].PlacementId;
            SW.GetResource<OpenWorldResourceNodeDeltaStore>().RecordDepleted(depletedPlacementId);

            scope.RunSeedSystem();

            Assert.That(AnyResourceNodeWithPlacementId(depletedPlacementId), Is.False);
        }

        [Test]
        public void DeltaCaptureSystem_WhenResourceNodeIsDepleted_RecordsPlacementId()
        {
            using var scope = new OpenWorldResourcesServerWorldScope();
            scope.RunSeedSystem();
            var entity = FirstResourceNode();
            var placementId = entity.Read<OpenWorldResourceNodeState>().PlacementId;
            ref var state = ref entity.Mut<OpenWorldResourceNodeState>();
            state.RemainingAmount = 0;

            new ServerOpenWorldResourceNodeDeltaCaptureSystem().Update();

            Assert.That(SW.GetResource<OpenWorldResourceNodeDeltaStore>().IsDepleted(placementId), Is.True);
        }

        [Test]
        public void DeltaStore_DoesNotStoreGeneratedMeshOrHeightmapData()
        {
            var storedTypes = typeof(OpenWorldResourceNodeDeltaStore)
                .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                .Select(field => field.FieldType)
                .Concat(typeof(OpenWorldResourceNodeDeltaStore)
                    .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                    .Select(property => property.PropertyType))
                .ToArray();

            Assert.That(storedTypes, Does.Not.Contain(typeof(GeneratedChunkData).ToString()));
            Assert.That(storedTypes, Does.Not.Contain(typeof(TerrainMeshData).ToString()));
            Assert.That(storedTypes.Any(type => type.IsArray), Is.False);
        }

        [Test]
        public void OpenWorldGenerationPresentation_DoesNotCreateResourceNodeEntities()
        {
            var root = Path.Combine(ProjectRoot(), "Assets", "Scripts", "StaticMlp", "Features", "OpenWorldGeneration", "Runtime", "Presentation");
            var offenders = Directory
                .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
                .Where(path =>
                {
                    var text = File.ReadAllText(path);
                    return text.IndexOf(nameof(OpenWorldResourceNodeFactory), StringComparison.Ordinal) >= 0
                           || text.IndexOf(nameof(OpenWorldResourceNodeNetworkEntity), StringComparison.Ordinal) >= 0
                           || text.IndexOf("OpenWorldResources", StringComparison.Ordinal) >= 0;
                })
                .Select(NormalizeRelativePath)
                .ToArray();

            Assert.That(offenders, Is.Empty);
        }

        private static int CountResourceNodes()
        {
            var count = 0;
            foreach (var _ in SW.Query<All<OpenWorldResourceNodeTag, OpenWorldResourceNodeState>>().Entities())
                count++;

            return count;
        }

        private static bool AnyResourceNodeWithPlacementId(long placementId)
        {
            foreach (var entity in SW.Query<All<OpenWorldResourceNodeTag, OpenWorldResourceNodeState>>().Entities())
            {
                if (entity.Read<OpenWorldResourceNodeState>().PlacementId == placementId)
                    return true;
            }

            return false;
        }

        private static SW.Entity FirstResourceNode()
        {
            foreach (var entity in SW.Query<All<OpenWorldResourceNodeTag, OpenWorldResourceNodeState>>().Entities())
                return entity;

            throw new InvalidOperationException("Expected at least one resource node.");
        }

        private static string ProjectRoot()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null)
            {
                var assetsPath = Path.Combine(directory.FullName, "Assets");
                if (Directory.Exists(assetsPath))
                    return directory.FullName;

                directory = directory.Parent;
            }

            throw new DirectoryNotFoundException("Could not resolve project root.");
        }

        private static string NormalizeRelativePath(string fullPath)
        {
            var root = ProjectRoot().Replace('\\', '/').TrimEnd('/') + "/";
            var normalized = Path.GetFullPath(fullPath).Replace('\\', '/');
            return normalized.Substring(root.Length);
        }

        private sealed class OpenWorldResourcesServerWorldScope : IDisposable
        {
            private readonly WorldGenerationRequest _request;

            public OpenWorldResourcesServerWorldScope()
            {
                if (SW.Status != WorldStatus.NotCreated)
                    SW.Destroy();

                _request = new WorldGenerationRequest(
                    new WorldGenerationSeed(12345),
                    new WorldChunkBounds(0, 0, 0, 0),
                    128f,
                    64,
                    0,
                    false,
                    4f);

                SW.Create(WorldConfig.Default());
                SW.Types().RegisterAll(
                    typeof(ServerWT).Assembly,
                    typeof(OpenWorldGenerationServerRuntime).Assembly,
                    typeof(OpenWorldResourcesGameplayFeature).Assembly);
                SW.Initialize();
                SW.RegisterCluster(1);
                SW.SetResource(new OpenWorldGenerationServerRuntime(new SimpleWorldGenerationService(), _request, 1));
                SW.SetResource(new OpenWorldResourceNodeFactory());
                SW.SetResource(new OpenWorldResourceNodeDeltaStore());
            }

            public GeneratedChunkData GenerateRequestedChunk()
            {
                return SW.GetResource<OpenWorldGenerationServerRuntime>().GenerationService.GenerateChunk(new WorldChunkId(0, 0), _request);
            }

            public void RunSeedSystem()
            {
                var system = new ServerOpenWorldResourceNodeSeedSystem();
                system.Init();
                system.Update();
            }

            public void Dispose()
            {
                if (SW.Status != WorldStatus.NotCreated)
                    SW.Destroy();
            }
        }
    }
}
