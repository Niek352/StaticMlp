using System;
using System.IO;
using System.Linq;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.EcsViews;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Features.OpenWorldResources;
using StaticMlp.Game.Components;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Replication.Generated;
using StaticMlp.Networking.Requests;
using StaticMlp.Networking.Transport;

namespace StaticMlp.Tests.OpenWorldResources
{
    public sealed class OpenWorldResourcesTests
    {
        [Test]
        public void ServerPlacementIndexSystem_RegistersGeneratedResourcePlacements()
        {
            using var scope = new OpenWorldResourcesServerWorldScope();
            var generated = scope.GenerateRequestedChunk();
            var expected = generated.ResourcePlacements.Length;

            scope.RunPlacementIndexSystem();

            Assert.That(expected, Is.GreaterThan(0));
            var index = SW.GetResource<OpenWorldPlacementIndexStore>();
            Assert.That(index.TryGetChunkPlacements(scope.ChunkId, out var placements), Is.True);
            Assert.That(placements.Length, Is.EqualTo(expected));
            Assert.That(index.TryGetPlacement(generated.ResourcePlacements[0].PlacementId, out var resolved), Is.True);
            Assert.That(resolved, Is.EqualTo(generated.ResourcePlacements[0]));
            Assert.That(CountResourceNodes(), Is.EqualTo(0));
        }

        [Test]
        public void ServerPlacementIndexSystem_WhenRerun_ReplacesPlacementFactsWithoutNetworkEntities()
        {
            using var scope = new OpenWorldResourcesServerWorldScope();

            scope.RunPlacementIndexSystem();
            var firstCount = SW.GetResource<OpenWorldPlacementIndexStore>().PlacementCount;
            scope.RunPlacementIndexSystem();

            Assert.That(SW.GetResource<OpenWorldPlacementIndexStore>().PlacementCount, Is.EqualTo(firstCount));
            Assert.That(CountResourceNodes(), Is.EqualTo(0));
        }

        [Test]
        public void OverlayStore_RegisterPlacement_CanResolveChunkByPlacementId()
        {
            using var scope = new OpenWorldResourcesServerWorldScope();
            var placementId = scope.GenerateRequestedChunk().ResourcePlacements[0].PlacementId;

            scope.RunPlacementIndexSystem();

            var overlayStore = SW.GetResource<OpenWorldChunkOverlayStore>();
            Assert.That(overlayStore.TryGetChunkId(placementId, out var chunkId), Is.True);
            Assert.That(chunkId, Is.EqualTo(scope.ChunkId));
        }

        [Test]
        public void OverlayStore_ApplyDepleted_IncrementsRevisionAndRecordsDirtyDelta()
        {
            using var scope = new OpenWorldResourcesServerWorldScope();
            var placement = scope.GenerateRequestedChunk().ResourcePlacements[0];
            var overlayStore = SW.GetResource<OpenWorldChunkOverlayStore>();

            var changed = overlayStore.TryApplyResourceState(scope.ChunkId, new OpenWorldResourceOverlayState
            {
                PlacementId = placement.PlacementId,
                KindIdValue = placement.KindId.Value,
                RemainingAmount = 0,
                Flags = OpenWorldResourceOverlayFlags.Depleted
            });

            Assert.That(changed, Is.True);
            Assert.That(overlayStore.IsDepleted(placement.PlacementId), Is.True);
            Assert.That(overlayStore.TryGet(scope.ChunkId, out var overlay), Is.True);
            Assert.That(overlay.Revision, Is.EqualTo(1));
            Assert.That(overlay.DirtyResourceDeltas.Count, Is.EqualTo(1));
            Assert.That(CountResourceNodes(), Is.EqualTo(0));
        }

        [Test]
        public void OverlayStore_ApplySameState_DoesNotIncrementRevision()
        {
            using var scope = new OpenWorldResourcesServerWorldScope();
            var placement = scope.GenerateRequestedChunk().ResourcePlacements[0];
            var state = new OpenWorldResourceOverlayState
            {
                PlacementId = placement.PlacementId,
                KindIdValue = placement.KindId.Value,
                RemainingAmount = 0,
                Flags = OpenWorldResourceOverlayFlags.Depleted
            };
            var overlayStore = SW.GetResource<OpenWorldChunkOverlayStore>();

            Assert.That(overlayStore.TryApplyResourceState(scope.ChunkId, state), Is.True);
            Assert.That(overlayStore.TryApplyResourceState(scope.ChunkId, state), Is.False);
            Assert.That(overlayStore.TryGet(scope.ChunkId, out var overlay), Is.True);
            Assert.That(overlay.Revision, Is.EqualTo(1));
            Assert.That(overlay.DirtyResourceDeltas.Count, Is.EqualTo(1));
        }

        [Test]
        public void OverlayStore_ClearDirty_RemovesDirtyDeltasButKeepsState()
        {
            using var scope = new OpenWorldResourcesServerWorldScope();
            var placement = scope.GenerateRequestedChunk().ResourcePlacements[0];
            var overlayStore = SW.GetResource<OpenWorldChunkOverlayStore>();
            overlayStore.TryApplyResourceState(scope.ChunkId, new OpenWorldResourceOverlayState
            {
                PlacementId = placement.PlacementId,
                KindIdValue = placement.KindId.Value,
                RemainingAmount = 0,
                Flags = OpenWorldResourceOverlayFlags.Depleted
            });

            var overlay = overlayStore.GetOrCreate(scope.ChunkId);
            overlay.ClearDirty();

            Assert.That(overlay.DirtyResourceDeltas.Count, Is.EqualTo(0));
            Assert.That(overlay.TryGetResource(placement.PlacementId, out var state), Is.True);
            Assert.That(state.Flags, Is.EqualTo(OpenWorldResourceOverlayFlags.Depleted));
        }

        [Test]
        public void PlacementIndex_RegisterChunkPlacements_ReplacesExistingChunkPlacements()
        {
            var index = new OpenWorldPlacementIndexStore();
            var chunkId = new WorldChunkId(0, 0);
            var first = new ResourcePlacement(101, new ResourcePlacementKindId(1), chunkId, UnityEngine.Vector3.zero, 0f, 1f);
            var second = new ResourcePlacement(202, new ResourcePlacementKindId(2), chunkId, UnityEngine.Vector3.one, 0f, 1f);

            index.RegisterChunkPlacements(chunkId, new[] { first });
            index.RegisterChunkPlacements(chunkId, new[] { second });

            Assert.That(index.TryGetPlacement(first.PlacementId, out _), Is.False);
            Assert.That(index.TryGetPlacement(second.PlacementId, out var resolved), Is.True);
            Assert.That(resolved, Is.EqualTo(second));
            Assert.That(index.PlacementCount, Is.EqualTo(1));
        }

        [Test]
        public void PlacementIndex_TryGetUnknownPlacement_ReturnsFalse()
        {
            var index = new OpenWorldPlacementIndexStore();

            Assert.That(index.TryGetPlacement(123456, out _), Is.False);
        }

        [Test]
        public void ClientResourceProxyIndex_CanResolveProxyByPlacementId()
        {
            var index = new ClientOpenWorldResourceProxyIndex();
            var gid = new EntityGID(1, 1, 0);

            index.Register(777, gid);

            Assert.That(index.TryGet(777, out var resolved), Is.True);
            Assert.That(resolved, Is.EqualTo(gid));
        }

        [Test]
        public void OverlayStore_DoesNotStoreGeneratedMeshOrHeightmapData()
        {
            var storedTypes = typeof(OpenWorldChunkOverlayStore)
                .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                .Select(field => field.FieldType)
                .Concat(typeof(OpenWorldChunkOverlay)
                    .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                    .Select(field => field.FieldType))
                .Concat(typeof(OpenWorldChunkOverlayStore)
                    .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                    .Select(property => property.PropertyType))
                .ToArray();

            Assert.That(storedTypes.Any(type => type == typeof(GeneratedChunkData)), Is.False);
            Assert.That(storedTypes.Any(type => type == typeof(TerrainMeshData)), Is.False);
        }

        [Test]
        public void ClientOnlyEntities_New_UsesLeasedSelfChunkInDependentClientWorld()
        {
            const uint leasedChunk = 321;
            if (CW.Status != WorldStatus.NotCreated)
                CW.Destroy();

            try
            {
                CW.Create(new WorldConfig
                {
                    Independent = false,
                    TrackCreated = true,
                    TrackingBufferSize = 64
                });
                CW.Types().RegisterAll(typeof(ClientCoreWT).Assembly);
                CW.Initialize();
                CW.SetResource(new ClientLocalChunkLease());
                CW.GetResource<ClientLocalChunkLease>().Replace(new[] { leasedChunk });
                CW.RegisterChunk(leasedChunk, ChunkOwnerType.Self, clusterId: 0);

                var entity = ClientOnlyEntities.New();

                Assert.That(entity.GID.Chunk, Is.EqualTo(leasedChunk));
                Assert.That(CW.GetChunkOwner(leasedChunk), Is.EqualTo(ChunkOwnerType.Self));
            }
            finally
            {
                if (CW.Status != WorldStatus.NotCreated)
                    CW.Destroy();
            }
        }

        [Test]
        public void ClientSpawnApplySystem_RemoteSpawnRegistersOtherChunkInDependentClientWorld()
        {
            NetArchetypeRegistry.Clear();
            ReplicationRegistry.Clear();
            var gid = CreateServerSpawn(out var entityType, out var snapshotPayload);

            if (CW.Status != WorldStatus.NotCreated)
                CW.Destroy();

            try
            {
                CW.Create(new WorldConfig
                {
                    Independent = false,
                    TrackCreated = true,
                    TrackingBufferSize = 64
                });
                CW.Types().RegisterAll(typeof(ClientCoreWT).Assembly, typeof(OpenWorldResourcesGameplayFeature).Assembly);
                CW.Initialize();
                CW.SetResource(new NetInbox());
                NetArchetypeRegistry.RegisterClient(999, _ => { });
                CW.GetResource<NetInbox>().Spawns.Add(new SpawnMessage
                {
                    Gid = gid,
                    EntityType = entityType,
                    Owner = new NetworkPeerId(1),
                    Authority = NetworkAuthority.Server,
                    NetworkArchetypeId = 999,
                    SnapshotPayload = snapshotPayload
                });

                new ClientSpawnApplySystem().Update();

                Assert.That(CW.ChunkIsRegistered(gid.Chunk), Is.True);
                Assert.That(CW.GetChunkOwner(gid.Chunk), Is.EqualTo(ChunkOwnerType.Other));
            }
            finally
            {
                NetArchetypeRegistry.Clear();
                if (CW.Status != WorldStatus.NotCreated)
                    CW.Destroy();
            }
        }

        [Test]
        public void SendExistingSpawns_ExcludesSpatialClusters()
        {
            if (SW.Status != WorldStatus.NotCreated)
                SW.Destroy();

            try
            {
                var bounds = new WorldChunkBounds(0, 0, 0, 0);
                var spatialCluster = OpenWorldSpatialClusterIds.ToClusterId(new WorldChunkId(0, 0), bounds);

                SW.Create(WorldConfig.Default());
                SW.Types().RegisterAll(typeof(ServerWT).Assembly);
                SW.Initialize();
                SW.RegisterCluster(spatialCluster);
                SW.SetResource(new NetOutbox());

                CreateNetworkedEntity(SW.NewEntity<Default>());
                CreateNetworkedEntity(SW.NewEntity<Default>(spatialCluster));

                SpawnBroadcaster.SendExistingSpawns(new NetworkPeerId(1));

                Assert.That(SW.GetResource<NetOutbox>().Packets.Count, Is.EqualTo(1));
            }
            finally
            {
                if (SW.Status != WorldStatus.NotCreated)
                    SW.Destroy();
            }
        }

        [Test]
        public void ServerStreaming_LoadsResourceChunkAndSendsSnapshot()
        {
            using var scope = new OpenWorldStreamingServerWorldScope(new WorldChunkBounds(0, 0, 0, 0));

            scope.UpdateStreaming();

            Assert.That(CountResourceNodes(), Is.EqualTo(0));
            Assert.That(SW.GetResource<NetOutbox>().Packets.Count, Is.EqualTo(1));
            Assert.That(DecodeFirstOutboxSnapshot().Kind, Is.EqualTo(ReplicationSnapshotKind.ClusterEntities));
            Assert.That(SW.ClusterIsRegistered(scope.ClusterId(new WorldChunkId(0, 0))), Is.True);
            Assert.That(
                SW.GetResource<OpenWorldServerChunkGeometryRuntime>().TryGet(new WorldChunkId(0, 0), out var geometry),
                Is.True);
            Assert.That(geometry.PhysicsMesh, Is.Not.Null);
        }

        [Test]
        public void ServerStreaming_UnloadsChunkAfterPeerLeavesInterest()
        {
            using var scope = new OpenWorldStreamingServerWorldScope(new WorldChunkBounds(0, 1, 0, 0));
            var firstChunk = new WorldChunkId(0, 0);
            var secondChunk = new WorldChunkId(1, 0);
            var firstCluster = scope.ClusterId(firstChunk);

            scope.UpdateStreaming();
            SW.GetResource<NetOutbox>().Clear();
            scope.SetPlayerChunk(secondChunk);
            scope.UpdateStreaming();

            ref var outbox = ref SW.GetResource<NetOutbox>();
            outbox.FlushNetworkEventBatches();
            Assert.That(outbox.Packets.Count, Is.EqualTo(2));
            Assert.That(SW.GetClusterLoadedChunks(firstCluster).Length, Is.EqualTo(0));
            Assert.That(CountResourceNodes(), Is.EqualTo(0));
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

        [Test]
        public void StaticResourceNodeClientArchetype_IsNotRegisteredByDefault()
        {
            using var scope = new OpenWorldResourcesClientPresentationWorldScope();

            new OpenWorldResourcesGameplayFeature().RegisterPrefabs();
            new OpenWorldResourcesPresentationFeature().RegisterPrefabs();
            var entity = CW.NewEntity<Default>();

            NetArchetypeRegistry.Apply(OpenWorldResourceNetworkArchetypeIds.ResourceNode, entity);

            Assert.That(entity.Has<OpenWorldResourceNodeTag>(), Is.False);
            Assert.That(entity.Has<ViewPath>(), Is.False);
            Assert.That(entity.Has<ViewTransform>(), Is.False);
            Assert.That(entity.Has<OpenWorldResourceNodeViewState>(), Is.False);
        }

        [Test]
        public void ClientResourceNodeViewStateSystem_BuildsViewStateFromReplicatedResourceNode()
        {
            using var scope = new OpenWorldResourcesClientPresentationWorldScope();
            var entity = CW.NewEntity<Default>();
            entity.Set<OpenWorldResourceNodeTag>();
            entity.Set(new OpenWorldResourceNodeState
            {
                PlacementId = 123,
                KindIdValue = 2,
                RemainingAmount = 7
            });
            entity.Set(new OpenWorldResourceNodeTransform
            {
                Position = new UnityEngine.Vector3(3f, 4f, 5f),
                YawDegrees = 45f,
                Scale = 1.7f
            });
            entity.Set(new ViewTransform
            {
                RenderRotation = UnityEngine.Quaternion.identity
            });
            entity.Set(new OpenWorldResourceNodeViewState());

            new ClientOpenWorldResourceNodeViewStateSystem().Update();

            ref readonly var viewTransform = ref entity.Read<ViewTransform>();
            Assert.That(viewTransform.RenderPosition, Is.EqualTo(new UnityEngine.Vector3(3f, 4f, 5f)));
            Assert.That(UnityEngine.Quaternion.Angle(viewTransform.RenderRotation, UnityEngine.Quaternion.Euler(0f, 45f, 0f)), Is.LessThan(0.001f));

            ref readonly var viewState = ref entity.Read<OpenWorldResourceNodeViewState>();
            Assert.That(viewState.KindIdValue, Is.EqualTo(2));
            Assert.That(viewState.RemainingAmount, Is.EqualTo(7));
            Assert.That(viewState.Scale, Is.EqualTo(1.7f));
        }

        [Test]
        public void PlacementIndexSystem_DoesNotSpawnResourceNodeNetworkEntities()
        {
            var path = Path.Combine(
                ProjectRoot(),
                "Assets",
                "Scripts",
                "StaticMlp",
                "Features",
                "OpenWorldResources",
                "Runtime",
                "Logic",
                "Systems",
                "Server",
                "ServerOpenWorldResourcePlacementIndexSystem.cs");
            var text = File.ReadAllText(path);

            Assert.That(text, Does.Not.Contain(nameof(OpenWorldResourceNodeFactory)));
            Assert.That(text, Does.Not.Contain(nameof(OpenWorldResourceNodeNetworkEntity)));
            Assert.That(text, Does.Not.Contain("SW.Query"));
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

        private static ReplicationSnapshotMessage DecodeFirstOutboxSnapshot()
        {
            var inbox = new NetInbox();
            var packet = SW.GetResource<NetOutbox>().Packets[0];
            Assert.That(PacketCodec.Decode(new NetworkPeerId(0), packet.Payload, inbox), Is.True);
            Assert.That(inbox.Snapshots.Count, Is.EqualTo(1));
            return inbox.Snapshots[0];
        }

        private static EntityGID CreateServerSpawn(out byte entityType, out byte[] snapshotPayload)
        {
            if (SW.Status != WorldStatus.NotCreated)
                SW.Destroy();

            try
            {
                SW.Create(WorldConfig.Default());
                SW.Types().RegisterAll(typeof(ServerWT).Assembly);
                SW.Initialize();
                var entity = SW.NewEntity<Default>();
                entity.Set(new NetworkIdentity
                {
                    Owner = new NetworkPeerId(1),
                    Authority = NetworkAuthority.Server,
                    NetworkArchetypeId = 999
                });
                entityType = entity.EntityType;
                snapshotPayload = ReplicationRegistry.CreateInitialStateSnapshot(entity, new NetworkPeerId(1));
                return entity.GID;
            }
            finally
            {
                if (SW.Status != WorldStatus.NotCreated)
                    SW.Destroy();
            }
        }

        private static void CreateNetworkedEntity(SW.Entity entity)
        {
            entity.Set(new NetworkIdentity
            {
                Owner = new NetworkPeerId(0),
                Authority = NetworkAuthority.Server,
                NetworkArchetypeId = 0
            });
            entity.Set<NetworkedTag>();
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
            private readonly WorldChunkId _chunkId = new(0, 0);
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
                ClusterId = OpenWorldSpatialClusterIds.ToClusterId(_chunkId, _request.Bounds);

                SW.Create(WorldConfig.Default());
                SW.Types().RegisterAll(
                    typeof(ServerWT).Assembly,
                    typeof(OpenWorldChunkLoadRequested).Assembly,
                    typeof(OpenWorldGenerationServerRuntime).Assembly,
                    typeof(OpenWorldResourcesGameplayFeature).Assembly);
                SW.Initialize();
                SW.RegisterCluster(ClusterId);
                SW.SetResource(new OpenWorldGenerationServerRuntime(_request, 1));
                SW.SetResource(new OpenWorldPlacementIndexStore());
                SW.SetResource(new OpenWorldChunkOverlayStore());
            }

            public WorldChunkId ChunkId => _chunkId;
            public ushort ClusterId { get; }

            public GeneratedChunkData GenerateRequestedChunk()
            {
                return new SimpleWorldGenerationService().GenerateChunk(_chunkId, _request);
            }

            public void RunPlacementIndexSystem()
            {
                var system = new ServerOpenWorldResourcePlacementIndexSystem();
                system.Init();
                var generated = GenerateRequestedChunk();
                SW.SendEvent(new OpenWorldChunkGenerationCompleted(
                    _chunkId,
                    _request.Lod,
                    GenerationOutputMask.Placements,
                    null,
                    null,
                    null,
                    generated.ResourcePlacements,
                    generated.SpawnPlacements));
                system.Update();
                system.Destroy();
                SW.Tick();
            }

            public void Dispose()
            {
                if (SW.Status != WorldStatus.NotCreated)
                    SW.Destroy();
            }
        }

        private sealed class OpenWorldResourcesClientPresentationWorldScope : IDisposable
        {
            public OpenWorldResourcesClientPresentationWorldScope()
            {
                NetArchetypeRegistry.Clear();
                ProjectionRegistry.Clear();

                if (CW.Status != WorldStatus.NotCreated)
                    CW.Destroy();

                new OpenWorldResourcesGameplayFeature().RegisterNetworkEvents();
                ProjectionRegistry.Register<OpenWorldResourceNodeState>();
                ProjectionRegistry.Register<OpenWorldResourceNodeTransform>();

                CW.Create(WorldConfig.Default());
                CW.Types().RegisterAll(
                    typeof(ClientCoreWT).Assembly,
                    typeof(ViewPath).Assembly,
                    typeof(ViewTransform).Assembly,
                    typeof(OpenWorldResourcesGameplayFeature).Assembly,
                    typeof(OpenWorldResourcesPresentationFeature).Assembly);
                ProjectionRegistry.RegisterClientWorldTypes();
                CW.Initialize();
            }

            public void Dispose()
            {
                NetArchetypeRegistry.Clear();
                ProjectionRegistry.Clear();

                if (CW.Status != WorldStatus.NotCreated)
                    CW.Destroy();
            }
        }

        private sealed class OpenWorldStreamingServerWorldScope : IDisposable
        {
            private readonly ServerOpenWorldResourcePlacementIndexSystem _resourcePlacementIndexSystem = new();
            private readonly ServerOpenWorldChunkInterestSystem _interestSystem = new();
            private readonly ServerOpenWorldChunkGenerationBridgeSystem _generationBridgeSystem = new();
            private readonly ServerOpenWorldChunkGenerationSystem _generationSystem = new();
            private readonly ServerOpenWorldChunkGeometryStoreSystem _geometryStoreSystem = new();
            private readonly ServerOpenWorldChunkGenerationCompleteSystem _generationCompleteSystem = new();
            private readonly ServerOpenWorldChunkSnapshotSystem _snapshotSystem = new();
            private readonly WorldGenerationRequest _request;
            private readonly EntityGID _player;
            private readonly NetworkPeerId _peer = new(1);

            public OpenWorldStreamingServerWorldScope(WorldChunkBounds bounds)
            {
                if (SW.Status != WorldStatus.NotCreated)
                    SW.Destroy();

                _request = new WorldGenerationRequest(
                    new WorldGenerationSeed(12345),
                    bounds,
                    128f,
                    64,
                    0,
                    false,
                    4f);

                NetworkEventRegistry.Clear();
                ReplicatedNetworkEventRegistry.RegisterNetworkEvents();
                ReplicatedComponentRegistration.RegisterReplicationComponents();
                ServerPeerRegistry.Clear();
                ServerPeerRegistry.Add(_peer);

                SW.Create(WorldConfig.Default());
                SW.Types().RegisterAll(
                    typeof(ServerWT).Assembly,
                    typeof(CharacterNetState).Assembly,
                    typeof(OpenWorldChunkLoadRequested).Assembly,
                    typeof(OpenWorldGenerationServerRuntime).Assembly,
                    typeof(OpenWorldResourcesGameplayFeature).Assembly);
                SW.Initialize();
                SW.SetResource(new OpenWorldGenerationServerRuntime(_request, 8, 0));
                SW.SetResource(new OpenWorldChunkGenerationRuntime(
                    _request.Seed,
                    _request.Bounds,
                    _request.ChunkWorldSize,
                    -7f,
                    _request.BaseQuadCount,
                    _request.AddSkirts,
                    _request.SkirtDepth));
                SW.SetResource(new OpenWorldChunkStreamingState());
                SW.SetResource(new OpenWorldServerChunkGeometryRuntime());
                SW.SetResource(new OpenWorldPlacementIndexStore());
                SW.SetResource(new OpenWorldChunkOverlayStore());
                SW.SetResource(new NetOutbox());

                var player = SW.NewEntity<Default>();
                player.Set<PlayerTag>();
                player.Set(new NetworkIdentity
                {
                    Owner = _peer,
                    Authority = NetworkAuthority.Owner,
                    NetworkArchetypeId = 0
                });
                player.Set(new CharacterNetState
                {
                    Position = new WorldChunkId(0, 0).GetWorldCenter(_request.ChunkWorldSize),
                    Rotation = UnityEngine.Quaternion.identity
                });
                _player = player.GID;
                _generationBridgeSystem.Init();
                _generationSystem.Init();
                _resourcePlacementIndexSystem.Init();
                _geometryStoreSystem.Init();
                _generationCompleteSystem.Init();
            }

            public ushort ClusterId(WorldChunkId chunkId)
            {
                return OpenWorldSpatialClusterIds.ToClusterId(chunkId, _request.Bounds);
            }

            public void SetPlayerChunk(WorldChunkId chunkId)
            {
                if (!_player.TryUnpack<ServerWT>(out var player))
                    throw new InvalidOperationException("Streaming test player is not loaded.");

                ref var state = ref player.Mut<CharacterNetState>();
                state.Position = chunkId.GetWorldCenter(_request.ChunkWorldSize);
            }

            public void UpdateStreaming()
            {
                for (var i = 0; i < 64; i++)
                {
                _interestSystem.Update();
                _generationBridgeSystem.Update();
                _generationSystem.Update();
                SW.GetResource<OpenWorldChunkGenerationRuntime>().CompleteScheduledJobs();
                _generationSystem.Update();
                _resourcePlacementIndexSystem.Update();
                    _geometryStoreSystem.Update();
                    _generationCompleteSystem.Update();
                    _snapshotSystem.Update();
                    SW.Tick();
                    _resourcePlacementIndexSystem.Update();
                    _geometryStoreSystem.Update();
                    _generationCompleteSystem.Update();
                    _snapshotSystem.Update();
                }
            }

            public void Dispose()
            {
                _generationCompleteSystem.Destroy();
                _geometryStoreSystem.Destroy();
                _resourcePlacementIndexSystem.Destroy();
                _generationSystem.Destroy();
                _generationBridgeSystem.Destroy();
                ServerPeerRegistry.Clear();
                NetworkEventRegistry.Clear();
                ReplicationRegistry.Clear();

                if (SW.Status != WorldStatus.NotCreated)
                    SW.Destroy();
            }
        }
    }
}
