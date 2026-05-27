using System;
using System.Collections;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.EcsViews;
using StaticMlp.Features.ResourcesInventoryMinimal;
using StaticMlp.Features.Settlement;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using UnityEngine;
using UnityEngine.TestTools;

namespace StaticMlp.Tests.ResourcesInventoryMinimal
{
    public sealed class ResourcePickupAuthorityTests
    {
        [Test]
        public void ServerPickupCollect_PlayerOutsideCollectionRadius_DoesNotCollect()
        {
            using var scope = new ServerResourcesInventoryWorldScope();
            var player = scope.CreatePlayer(new Vector3(3f, 0f, 0f));
            var pickup = scope.CreatePickup(Vector3.zero, player.GID, amount: 3);
            var system = new ServerResourcePickupCollectSystem();

            system.Update();

            Assert.That(pickup.Read<ResourcePickup>().IsPickedUp, Is.False);
            Assert.That(pickup.Has<ResourcePickupDespawnTimer>(), Is.False);
            Assert.That(ResourcesInventoryAccess.GetAmount(player, ResourceCatalog.WoodId), Is.EqualTo(0));
        }

        [Test]
        public void ServerPickupCollect_PlayerInsideCollectionRadius_MarksPickedUp()
        {
            using var scope = new ServerResourcesInventoryWorldScope();
            var player = scope.CreatePlayer(new Vector3(1f, 0f, 0f));
            var pickup = scope.CreatePickup(Vector3.zero, player.GID, amount: 3);
            var system = new ServerResourcePickupCollectSystem();

            system.Update();

            Assert.That(pickup.Read<ResourcePickup>().IsPickedUp, Is.True);
            Assert.That(pickup.Read<ResourcePickup>().CollectorPlayer, Is.EqualTo(player.GID));
            Assert.That(pickup.Has<ResourcePickupDespawnTimer>(), Is.True);
            Assert.That(pickup.Read<ResourcePickupDespawnTimer>().DespawnAtTick, Is.EqualTo(scope.SimulationTime.DeadlineAfter(2f)));
            Assert.That(ResourcesInventoryAccess.GetAmount(player, ResourceCatalog.WoodId), Is.EqualTo(3));
        }

        [Test]
        public void ServerPickupCollect_FullWoodStack_UsesNextInventorySlot()
        {
            using var scope = new ServerResourcesInventoryWorldScope();
            var player = scope.CreatePlayer(new Vector3(1f, 0f, 0f));
            var rejected = ResourcesInventoryAccess.Add(
                player,
                new ResourceAmount(ResourceCatalog.WoodId, ResourcesInventory.MAX_STACK_AMOUNT));
            var pickup = scope.CreatePickup(Vector3.zero, player.GID, amount: 3);
            var system = new ServerResourcePickupCollectSystem();

            system.Update();

            ref readonly var rows = ref player.Ref<SW.Multi<CarriedResourceEntry>>();
            Assert.That(rejected, Is.EqualTo(0));
            Assert.That(pickup.Read<ResourcePickup>().IsPickedUp, Is.True);
            Assert.That(ResourcesInventoryAccess.GetAmount(player, ResourceCatalog.WoodId), Is.EqualTo(23));
            Assert.That(rows.Length, Is.EqualTo(2));
            Assert.That(rows.Get(0).Amount, Is.EqualTo(ResourcesInventory.MAX_STACK_AMOUNT));
            Assert.That(rows.Get(1).Amount, Is.EqualTo(3));
        }

        [Test]
        public void ServerPickupCollect_PartialOverflow_LeavesRemainderWithoutCollector()
        {
            using var scope = new ServerResourcesInventoryWorldScope();
            var player = scope.CreatePlayer(new Vector3(1f, 0f, 0f));
            var rejected = ResourcesInventoryAccess.Add(
                player,
                new ResourceAmount(ResourceCatalog.WoodId, ResourcesInventory.MAX_SLOTS * ResourcesInventory.MAX_STACK_AMOUNT - 1));
            var pickup = scope.CreatePickup(Vector3.zero, player.GID, amount: 3);
            var system = new ServerResourcePickupCollectSystem();

            system.Update();

            ref readonly var pickupState = ref pickup.Read<ResourcePickup>();
            Assert.That(rejected, Is.EqualTo(0));
            Assert.That(pickupState.IsPickedUp, Is.False);
            Assert.That(pickupState.Amount, Is.EqualTo(2));
            Assert.That(pickupState.CollectorPlayer, Is.EqualTo(default(EntityGID)));
            Assert.That(pickup.Has<ResourcePickupDespawnTimer>(), Is.False);
            Assert.That(ResourcesInventoryAccess.GetAmount(player, ResourceCatalog.WoodId), Is.EqualTo(400));
        }

        [Test]
        public void ServerPickupCollect_AllWoodSlotsFull_DoesNotCollect()
        {
            using var scope = new ServerResourcesInventoryWorldScope();
            var player = scope.CreatePlayer(new Vector3(1f, 0f, 0f));
            var rejected = ResourcesInventoryAccess.Add(
                player,
                new ResourceAmount(ResourceCatalog.WoodId, ResourcesInventory.MAX_SLOTS * ResourcesInventory.MAX_STACK_AMOUNT));
            var pickup = scope.CreatePickup(Vector3.zero, player.GID, amount: 3);
            var system = new ServerResourcePickupCollectSystem();

            system.Update();

            Assert.That(rejected, Is.EqualTo(0));
            Assert.That(pickup.Read<ResourcePickup>().IsPickedUp, Is.False);
            Assert.That(pickup.Has<ResourcePickupDespawnTimer>(), Is.False);
            Assert.That(ResourcesInventoryAccess.GetAmount(player, ResourceCatalog.WoodId), Is.EqualTo(400));
        }

        [Test]
        public void ServerPickupCleanup_BeforeDespawnTick_KeepsPickup()
        {
            using var scope = new ServerResourcesInventoryWorldScope();
            scope.SetServerTick(104);
            var pickup = scope.CreatePickup(Vector3.zero, default, amount: 3);
            pickup.Set(new ResourcePickupDespawnTimer { DespawnAtTick = 105 });
            var gid = pickup.GID;
            var system = new ServerResourcePickupCleanupSystem();

            system.Update();

            Assert.That(gid.TryUnpack<ServerWT>(out _), Is.True);
        }

        [Test]
        public void ServerPickupCleanup_AtDespawnTick_DespawnsPickup()
        {
            using var scope = new ServerResourcesInventoryWorldScope();
            scope.SetServerTick(105);
            var pickup = scope.CreatePickup(Vector3.zero, default, amount: 3);
            pickup.Set(new ResourcePickupDespawnTimer { DespawnAtTick = 105 });
            var gid = pickup.GID;
            var system = new ServerResourcePickupCleanupSystem();

            system.Update();

            Assert.That(gid.TryUnpack<ServerWT>(out _), Is.False);
        }

        [Test]
        public void ClientPickupMagnet_UnpickedPickupInsideOldMagnetRadius_DoesNotMove()
        {
            using var scope = new ClientResourcesInventoryWorldScope();
            scope.CreateLocalPlayer(new Vector3(3f, 0f, 0f));
            var pickup = scope.CreatePickup(Vector3.zero, isPickedUp: false);
            var system = new ClientResourcePickupMagnetViewSystem();

            system.Update();

            Assert.That(pickup.Read<ViewTransform>().RenderPosition, Is.EqualTo(Vector3.zero));
            Assert.That(pickup.Read<ResourcePickupViewState>().IsMagnetized, Is.False);
            Assert.That(pickup.Read<ResourcePickupViewState>().IsConsumed, Is.False);
        }

        [UnityTest]
        public IEnumerator ClientPickupMagnet_PickedUpPickup_MovesTowardCollector()
        {
            using var scope = new ClientResourcesInventoryWorldScope();
            var previousCaptureDeltaTime = Time.captureDeltaTime;
            Time.captureDeltaTime = 1f / 60f;

            try
            {
                yield return null;

                var playerPosition = new Vector3(1f, 0f, 0f);
                var collector = scope.CreateLocalPlayer(playerPosition);
                var pickup = scope.CreatePickup(Vector3.zero, isPickedUp: true, collectorPlayer: collector.GID);
                var system = new ClientResourcePickupMagnetViewSystem();

                system.Update();

                var renderPosition = pickup.Read<ViewTransform>().RenderPosition;
                Assert.That(renderPosition, Is.Not.EqualTo(Vector3.zero));
                Assert.That(
                    Vector3.Distance(renderPosition, playerPosition),
                    Is.LessThan(Vector3.Distance(Vector3.zero, playerPosition)));
                Assert.That(pickup.Read<ResourcePickupViewState>().IsMagnetized, Is.True);
                Assert.That(pickup.Read<ResourcePickupViewState>().IsConsumed, Is.False);
            }
            finally
            {
                Time.captureDeltaTime = previousCaptureDeltaTime;
            }
        }

        [Test]
        public void ClientPickupMagnet_PickedUpPickupAtPlayer_MarksVisualConsumed()
        {
            using var scope = new ClientResourcesInventoryWorldScope();
            var playerPosition = new Vector3(1f, 0f, 0f);
            var collector = scope.CreateLocalPlayer(playerPosition);
            var pickup = scope.CreatePickup(playerPosition, isPickedUp: true, collectorPlayer: collector.GID);
            var system = new ClientResourcePickupMagnetViewSystem();

            system.Update();

            ref readonly var viewState = ref pickup.Read<ResourcePickupViewState>();
            Assert.That(viewState.IsMagnetized, Is.True);
            Assert.That(viewState.IsConsumed, Is.True);
        }

        [UnityTest]
        public IEnumerator ClientPickupMagnet_PickedUpByRemoteCollector_MovesTowardCollectorNotLocalPlayer()
        {
            using var scope = new ClientResourcesInventoryWorldScope();
            var previousCaptureDeltaTime = Time.captureDeltaTime;
            Time.captureDeltaTime = 1f / 60f;

            try
            {
                yield return null;

                var localPlayerPosition = new Vector3(-10f, 0f, 0f);
                scope.CreateLocalPlayer(localPlayerPosition);
                var collector = scope.CreateRemoteCollector(new Vector3(1f, 0f, 0f));
                var pickup = scope.CreatePickup(Vector3.zero, isPickedUp: true, collectorPlayer: collector.GID);
                var system = new ClientResourcePickupMagnetViewSystem();

                system.Update();

                var renderPosition = pickup.Read<ViewTransform>().RenderPosition;
                Assert.That(
                    Vector3.Distance(renderPosition, collector.Read<ViewTransform>().RenderPosition),
                    Is.LessThan(Vector3.Distance(Vector3.zero, collector.Read<ViewTransform>().RenderPosition)));
                Assert.That(
                    Vector3.Distance(renderPosition, localPlayerPosition),
                    Is.GreaterThan(Vector3.Distance(Vector3.zero, localPlayerPosition)));
            }
            finally
            {
                Time.captureDeltaTime = previousCaptureDeltaTime;
            }
        }

        [Test]
        public void ResourcePickupViewPart_ConsumedState_HidesOrb()
        {
            using var scope = new ClientResourcesInventoryWorldScope();
            var collector = scope.CreateLocalPlayer(Vector3.zero);
            var pickup = scope.CreatePickup(Vector3.zero, isPickedUp: true, collectorPlayer: collector.GID);
            ref var viewState = ref pickup.Mut<ResourcePickupViewState>();
            viewState.IsConsumed = true;

            var gameObject = new GameObject("Resource Pickup View Test");
            try
            {
                var viewPart = gameObject.AddComponent<ResourcePickupViewPart>();
                viewPart.OnBind(new TestEntityView(pickup));

                var orb = gameObject.transform.Find("Resource Pickup Orb");
                Assert.That(orb, Is.Not.Null);
                Assert.That(orb.gameObject.activeSelf, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        private sealed class ServerResourcesInventoryWorldScope : IDisposable
        {
            public ServerResourcesInventoryWorldScope()
            {
                if (SW.Status != WorldStatus.NotCreated)
                    SW.Destroy();

                SW.Create(WorldConfig.Default());
                SW.Types().RegisterAll(
                    typeof(ServerWT).Assembly,
                    typeof(PlayerTag).Assembly,
                    typeof(CharacterNetState).Assembly,
                    typeof(ResourcePickup).Assembly);
                SW.Initialize();
                SW.SetResource(ResourcesInventoryConfig.CreateDefault());
                SW.SetResource(new SimulationTime
                {
                    FixedStepSeconds = 1f / 30f,
                    ServerTick = 100
                });
            }

            public SimulationTime SimulationTime => SW.GetResource<SimulationTime>();

            public void SetServerTick(uint serverTick)
            {
                SimulationTime.ServerTick = serverTick;
            }

            public SW.Entity CreatePlayer(Vector3 position)
            {
                var player = SW.NewEntity<Default>();
                player.Set<PlayerTag>();
                player.Set(new CharacterNetState
                {
                    Position = position,
                    Rotation = Quaternion.identity
                });
                ResourcesInventoryAccess.Initialize(player, ResourcesInventory.MAX_SLOTS);
                return player;
            }

            public SW.Entity CreatePickup(Vector3 position, EntityGID sourcePlayer, int amount)
            {
                var pickup = SW.NewEntity<Default>();
                pickup.Set<ServerOwned>();
                pickup.Set<ResourcePickupTag>();
                pickup.Set(new ResourcePickup
                {
                    ResourceId = ResourceCatalog.WoodId.Value,
                    Amount = amount,
                    Position = position
                });
                pickup.Set(new ResourcePickupSource
                {
                    SourcePlayer = sourcePlayer,
                    PlacementId = 10
                });
                return pickup;
            }

            public void Dispose()
            {
                if (SW.Status != WorldStatus.NotCreated)
                    SW.Destroy();
            }
        }

        private sealed class ClientResourcesInventoryWorldScope : IDisposable
        {
            public ClientResourcesInventoryWorldScope()
            {
                if (CW.Status != WorldStatus.NotCreated)
                    CW.Destroy();

                CW.Create(WorldConfig.Default());
                CW.Types().RegisterAll(
                    typeof(ClientCoreWT).Assembly,
                    typeof(PlayerTag).Assembly,
                    typeof(ViewTransform).Assembly,
                    typeof(ResourcePickup).Assembly,
                    typeof(ResourcesInventoryPresentationFeature).Assembly);
                CW.Initialize();
                CW.SetResource(ResourcesInventoryConfig.CreateDefault());
            }

            public CW.Entity CreateLocalPlayer(Vector3 renderPosition)
            {
                var player = CW.NewEntity<Default>();
                player.Set<LocalOwned>();
                player.Set<PlayerTag>();
                player.Set(new ViewTransform
                {
                    RenderPosition = renderPosition,
                    RenderRotation = Quaternion.identity
                });
                return player;
            }

            public CW.Entity CreateRemoteCollector(Vector3 renderPosition)
            {
                var player = CW.NewEntity<Default>();
                player.Set<PlayerTag>();
                player.Set(new ViewTransform
                {
                    RenderPosition = renderPosition,
                    RenderRotation = Quaternion.identity
                });
                return player;
            }

            public CW.Entity CreatePickup(Vector3 position, bool isPickedUp, EntityGID collectorPlayer = default)
            {
                var pickup = CW.NewEntity<Default>();
                pickup.Set(new ResourcePickup
                {
                    ResourceId = ResourceCatalog.WoodId.Value,
                    Amount = 3,
                    Position = position,
                    CollectorPlayer = collectorPlayer,
                    IsPickedUp = isPickedUp
                });
                pickup.Set(new ViewTransform
                {
                    RenderPosition = position,
                    RenderRotation = Quaternion.identity
                });
                pickup.Set(new ResourcePickupViewState());
                return pickup;
            }

            public void Dispose()
            {
                if (CW.Status != WorldStatus.NotCreated)
                    CW.Destroy();
            }
        }

        private sealed class TestEntityView : IEntityView
        {
            public TestEntityView(CW.Entity entity)
            {
                Entity = entity;
            }

            public CW.Entity Entity { get; private set; }

            public void Bind(CW.Entity entity)
            {
                Entity = entity;
            }

            public void Unbind()
            {
            }

            public void Apply<TComponent>(in TComponent component)
                where TComponent : struct, IViewComponent
            {
            }
        }
    }
}
