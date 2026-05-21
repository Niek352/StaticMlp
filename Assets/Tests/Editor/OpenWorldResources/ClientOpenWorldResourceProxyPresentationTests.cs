using System;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.EcsViews;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Features.OpenWorldResources;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StaticMlp.Tests.OpenWorldResources
{
    public sealed class ClientOpenWorldResourceProxyPresentationTests
    {
        [Test]
        public void ProxySpawn_WritesResourceNodeViewStateWithMaxAmountAndFlags()
        {
            using var scope = new ClientOpenWorldResourcesWorldScope();
            var placement = CreatePlacement(placementId: 101, kindId: 2);

            SpawnProxy(placement);

            var proxy = GetProxy(placement.PlacementId);
            ref readonly var viewState = ref proxy.Read<OpenWorldResourceNodeViewState>();
            Assert.That(viewState.KindIdValue, Is.EqualTo(2));
            Assert.That(viewState.RemainingAmount, Is.EqualTo(8));
            Assert.That(viewState.MaxAmount, Is.EqualTo(8));
            Assert.That(viewState.Flags, Is.EqualTo(OpenWorldResourceOverlayFlags.None));
            Assert.That(viewState.Scale, Is.EqualTo(1.25f));
        }

        [Test]
        public void ResourceNodeViewBind_CreatesRuntimeViewWithoutResourcesPrefab()
        {
            using var scope = new ClientOpenWorldResourcesWorldScope();
            var placement = CreatePlacement(placementId: 103, kindId: 2);
            SpawnProxy(placement);

            var proxy = GetProxy(placement.PlacementId);
            Assert.That(proxy.Has<ViewPath>(), Is.False);

            var system = new ClientOpenWorldResourceNodeViewBindSystem();
            system.Update();

            Assert.That(proxy.Has<View>(), Is.True);
            ref readonly var viewComponent = ref proxy.Read<View>();
            Assert.That(viewComponent.Value, Is.TypeOf<OpenWorldResourceNodeRuntimeView>());

            var view = (OpenWorldResourceNodeRuntimeView)viewComponent.Value;
            Assert.That(view.GetComponent<TransformViewComponent>(), Is.Not.Null);
            Assert.That(view.GetComponent<OpenWorldResourceNodeViewPart>(), Is.Not.Null);
            Assert.That(view.transform.position, Is.EqualTo(placement.Position));
            Assert.That(view.transform.Find("Resource Node Visual/Stone Boulder"), Is.Not.Null);
            
        }

        [Test]
        public void OverlayUpdates_WriteResourceNodeViewStateAmountAndFlags()
        {
            using var scope = new ClientOpenWorldResourcesWorldScope();
            var placement = CreatePlacement(placementId: 102, kindId: 2);
            SpawnProxy(placement);

            var system = new ClientOpenWorldChunkOverlayApplySystem();
            system.Init();
            try
            {
                var absolute = new OpenWorldChunkOverlayAbsolute
                {
                    ChunkId = placement.ChunkId,
                    Revision = 1,
                    ResourceStates = new[]
                    {
                        new OpenWorldResourceOverlayState
                        {
                            PlacementId = placement.PlacementId,
                            KindIdValue = 2,
                            RemainingAmount = 4,
                            Flags = OpenWorldResourceOverlayFlags.Respawning,
                            RespawnTick = 33
                        }
                    }
                };
                CW.SendEvent(new NetworkEventFromServer<OpenWorldChunkOverlayAbsolute>(new NetworkPeerId(1), absolute));
                system.Update();

                var proxy = GetProxy(placement.PlacementId);
                ref readonly var absoluteViewState = ref proxy.Read<OpenWorldResourceNodeViewState>();
                Assert.That(absoluteViewState.RemainingAmount, Is.EqualTo(4));
                Assert.That(absoluteViewState.MaxAmount, Is.EqualTo(8));
                Assert.That(absoluteViewState.Flags, Is.EqualTo(OpenWorldResourceOverlayFlags.Respawning));

                var delta = new OpenWorldChunkOverlayDelta
                {
                    ChunkId = placement.ChunkId,
                    BasisRevision = 1,
                    Revision = 2,
                    ResourceDeltas = new[]
                    {
                        new OpenWorldResourceOverlayDelta
                        {
                            PlacementId = placement.PlacementId,
                            RemainingAmount = 2,
                            Flags = OpenWorldResourceOverlayFlags.Hidden,
                            RespawnTick = 44
                        }
                    }
                };
                CW.SendEvent(new NetworkEventFromServer<OpenWorldChunkOverlayDelta>(new NetworkPeerId(1), delta));
                system.Update();

                ref readonly var deltaViewState = ref proxy.Read<OpenWorldResourceNodeViewState>();
                Assert.That(deltaViewState.RemainingAmount, Is.EqualTo(2));
                Assert.That(deltaViewState.MaxAmount, Is.EqualTo(8));
                Assert.That(deltaViewState.Flags, Is.EqualTo(OpenWorldResourceOverlayFlags.Hidden));
            }
            finally
            {
                system.Destroy();
            }
        }

        private static ResourcePlacement CreatePlacement(long placementId, ushort kindId)
        {
            return new ResourcePlacement(
                placementId,
                new ResourcePlacementKindId(kindId),
                new WorldChunkId(1, 2),
                new Vector3(4f, 0f, 6f),
                45f,
                1.25f);
        }

        private static void SpawnProxy(ResourcePlacement placement)
        {
            var system = new ClientOpenWorldResourceProxySpawnSystem();
            system.Init();
            try
            {
                CW.SendEvent(new OpenWorldChunkGenerationCompleted(
                    placement.ChunkId,
                    128f,
                    0,
                    default,
                    default,
                    default,
                    default,
                    new[] { placement },
                    Array.Empty<SpawnPlacement>()));
                system.Update();
            }
            finally
            {
                system.Destroy();
            }
        }

        private static CW.Entity GetProxy(long placementId)
        {
            var proxyIndex = CW.GetResource<ClientOpenWorldResourceProxyIndex>();
            Assert.That(proxyIndex.TryGet(placementId, out var gid), Is.True);
            Assert.That(gid.TryUnpack<ClientCoreWT>(out var entity), Is.True);
            return entity;
        }

        private sealed class ClientOpenWorldResourcesWorldScope : IDisposable
        {
            public ClientOpenWorldResourcesWorldScope()
            {
                if (CW.Status != WorldStatus.NotCreated)
                    CW.Destroy();

                NetworkRuntime.LocalPeerId = new NetworkPeerId(1);
                NetworkEventRegistry.Clear();
                new OpenWorldResourcesGameplayFeature().RegisterNetworkEvents();

                CW.Create(WorldConfig.Default());
                CW.Types().RegisterAll(
                    typeof(ViewTransform).Assembly,
                    typeof(ClientCoreWT).Assembly,
                    typeof(OpenWorldChunkGenerationCompleted).Assembly,
                    typeof(OpenWorldResourcesGameplayFeature).Assembly,
                    typeof(OpenWorldResourcesPresentationFeature).Assembly,
                    typeof(ViewPath).Assembly);
                NetworkEventRegistry.RegisterClientWorldTypes();
                CW.Initialize();

                const uint clientLocalChunk = 501;
                CW.SetResource(new ClientLocalChunkLease());
                CW.GetResource<ClientLocalChunkLease>().Replace(new[] { clientLocalChunk });
                CW.RegisterChunk(clientLocalChunk, ChunkOwnerType.Self, clusterId: 0);
                CW.SetResource(new NetOutbox());
                CW.SetResource(new OpenWorldPlacementIndexStore());
                CW.SetResource(new OpenWorldChunkOverlayStore());
                CW.SetResource(new ClientOpenWorldResourceProxyIndex());
            }

            public void Dispose()
            {
                if (CW.Status != WorldStatus.NotCreated)
                    CW.Destroy();
            }
        }
    }
}
