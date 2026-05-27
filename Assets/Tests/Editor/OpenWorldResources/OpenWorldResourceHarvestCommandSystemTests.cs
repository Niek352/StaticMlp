using System;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.Effects;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Features.OpenWorldResources;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Shared;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Game.Input;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Tests.OpenWorldResources
{
    public sealed class OpenWorldResourceHarvestCommandSystemTests
    {
        [Test]
        public void ServerCommand_ValidHarvest_ReducesOverlayAndEmitsHarvestedEvent()
        {
            using var scope = new OpenWorldResourcesHarvestWorldScope(createServer: true, createClient: false);
            var peer = new NetworkPeerId(7);
            scope.CreateServerPlayer(peer, Vector3.zero);
            var placement = CreatePlacement(101, new Vector3(2f, 0f, 0f));
            scope.RegisterServerPlacement(placement);

            var count = RunServerCommand(peer, CreateCommand(placement.PlacementId, placement.Position), out var harvested);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(harvested.SourcePlayer, Is.Not.EqualTo(default(EntityGID)));
            Assert.That(harvested.PlacementId, Is.EqualTo(placement.PlacementId));
            Assert.That(harvested.HitPointXQ, Is.EqualTo(200));
            Assert.That(harvested.HitPointYQ, Is.EqualTo(0));
            Assert.That(harvested.HitPointZQ, Is.EqualTo(0));
            Assert.That(harvested.Resource.Id, Is.EqualTo(ResourceCatalog.OreId));
            Assert.That(harvested.Resource.Amount, Is.EqualTo(1));
            Assert.That(harvested.WasDepleted, Is.False);

            var overlayStore = SW.GetResource<OpenWorldChunkOverlayStore>();
            Assert.That(overlayStore.TryGetResource(placement.PlacementId, out var state), Is.True);
            Assert.That(state.RemainingAmount, Is.EqualTo(7));
            Assert.That(state.Flags, Is.EqualTo(OpenWorldResourceOverlayFlags.None));
        }

        [TestCase((ushort)1, (ushort)1)]
        [TestCase((ushort)2, (ushort)10)]
        [TestCase((ushort)3, (ushort)12)]
        [TestCase((ushort)4, (ushort)11)]
        public void ServerCommand_ProfileKind_EmitsConfiguredHarvestResource(ushort kindId, ushort expectedResourceId)
        {
            using var scope = new OpenWorldResourcesHarvestWorldScope(createServer: true, createClient: false);
            var peer = new NetworkPeerId(17);
            scope.CreateServerPlayer(peer, Vector3.zero);
            var placement = CreatePlacement(110 + kindId, new Vector3(2f, 0f, 0f), kindId);
            scope.RegisterServerPlacement(placement);

            var count = RunServerCommand(peer, CreateCommand(placement.PlacementId, placement.Position), out var harvested);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(harvested.Resource.Id, Is.EqualTo(new ResourceId(expectedResourceId)));
            Assert.That(harvested.Resource.Amount, Is.EqualTo(1));
        }

        [Test]
        public void ServerCommand_TwoValidCommandsInSameTick_AcceptsOnlyOneHarvest()
        {
            using var scope = new OpenWorldResourcesHarvestWorldScope(createServer: true, createClient: false);
            var peer = new NetworkPeerId(13);
            scope.CreateServerPlayer(peer, Vector3.zero);
            var placement = CreatePlacement(106, new Vector3(2f, 0f, 0f));
            scope.RegisterServerPlacement(placement);

            var firstCount = RunServerCommand(peer, CreateCommand(placement.PlacementId, placement.Position), out _);
            var secondCount = RunServerCommand(peer, CreateCommand(placement.PlacementId, placement.Position), out _);

            Assert.That(firstCount, Is.EqualTo(1));
            Assert.That(secondCount, Is.EqualTo(0));
            Assert.That(SW.GetResource<OpenWorldChunkOverlayStore>().TryGetResource(placement.PlacementId, out var state), Is.True);
            Assert.That(state.RemainingAmount, Is.EqualTo(7));
        }

        [Test]
        public void ServerCommand_AfterHarvestCooldown_AcceptsSecondHarvest()
        {
            using var scope = new OpenWorldResourcesHarvestWorldScope(createServer: true, createClient: false);
            var peer = new NetworkPeerId(14);
            scope.CreateServerPlayer(peer, Vector3.zero);
            var placement = CreatePlacement(107, new Vector3(2f, 0f, 0f));
            scope.RegisterServerPlacement(placement);

            var firstCount = RunServerCommand(peer, CreateCommand(placement.PlacementId, placement.Position), out _);
            scope.AdvanceSimulationSeconds(0.4f);
            var secondCount = RunServerCommand(peer, CreateCommand(placement.PlacementId, placement.Position), out _);

            Assert.That(firstCount, Is.EqualTo(1));
            Assert.That(secondCount, Is.EqualTo(1));
            Assert.That(SW.GetResource<OpenWorldChunkOverlayStore>().TryGetResource(placement.PlacementId, out var state), Is.True);
            Assert.That(state.RemainingAmount, Is.EqualTo(6));
        }

        [Test]
        public void ServerCommand_ClientHitPointInsideRange_EmitsAuthoritativePlacementPosition()
        {
            using var scope = new OpenWorldResourcesHarvestWorldScope(createServer: true, createClient: false);
            var peer = new NetworkPeerId(15);
            scope.CreateServerPlayer(peer, Vector3.zero);
            var placement = CreatePlacement(108, new Vector3(2.25f, 0f, 0f));
            scope.RegisterServerPlacement(placement);

            var count = RunServerCommand(peer, CreateCommand(placement.PlacementId, new Vector3(3.5f, 0f, 0f)), out var harvested);

            Assert.That(count, Is.EqualTo(1));
            Assert.That(harvested.HitPointXQ, Is.EqualTo(225));
            Assert.That(harvested.HitPointYQ, Is.EqualTo(0));
            Assert.That(harvested.HitPointZQ, Is.EqualTo(0));
        }

        [Test]
        public void ServerCommand_DepletedResource_CreatesHazardDamageEffect()
        {
            using var scope = new OpenWorldResourcesHarvestWorldScope(createServer: true, createClient: false);
            var peer = new NetworkPeerId(16);
            scope.CreateServerPlayer(peer, Vector3.zero);
            var placement = CreatePlacement(109, new Vector3(2f, 0f, 0f));
            scope.RegisterServerPlacement(placement);
            var target = scope.CreateServerCharacterWithHealth(new Vector3(2.5f, 0f, 0f), current: 100f, max: 100f);
            Assert.That(SW.GetResource<OpenWorldChunkOverlayStore>().TryApplyResourceState(placement.ChunkId, new OpenWorldResourceOverlayState
            {
                PlacementId = placement.PlacementId,
                KindIdValue = placement.KindId.Value,
                RemainingAmount = 1,
                Flags = OpenWorldResourceOverlayFlags.None
            }), Is.True);

            var hazardSystem = new ServerOpenWorldResourceHazardSystem();
            hazardSystem.Init();
            try
            {
                var count = RunServerCommand(peer, CreateCommand(placement.PlacementId, placement.Position), out var harvested);
                hazardSystem.Update();

                Assert.That(count, Is.EqualTo(1));
                Assert.That(harvested.WasDepleted, Is.True);
                Assert.That(CountDamageEffectsFor(target.GID), Is.EqualTo(1));
            }
            finally
            {
                hazardSystem.Destroy();
            }
        }

        [Test]
        public void ServerCommand_UnknownPlacement_IsRejected()
        {
            using var scope = new OpenWorldResourcesHarvestWorldScope(createServer: true, createClient: false);
            var peer = new NetworkPeerId(8);
            scope.CreateServerPlayer(peer, Vector3.zero);

            var count = RunServerCommand(peer, CreateCommand(999, Vector3.zero), out _);

            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public void ServerCommand_DepletedResource_IsRejectedWithoutMutation()
        {
            using var scope = new OpenWorldResourcesHarvestWorldScope(createServer: true, createClient: false);
            var peer = new NetworkPeerId(9);
            scope.CreateServerPlayer(peer, Vector3.zero);
            var placement = CreatePlacement(102, new Vector3(2f, 0f, 0f));
            scope.RegisterServerPlacement(placement);
            var overlayStore = SW.GetResource<OpenWorldChunkOverlayStore>();
            Assert.That(overlayStore.TryApplyResourceState(placement.ChunkId, new OpenWorldResourceOverlayState
            {
                PlacementId = placement.PlacementId,
                KindIdValue = placement.KindId.Value,
                RemainingAmount = 0,
                Flags = OpenWorldResourceOverlayFlags.Depleted
            }), Is.True);

            var count = RunServerCommand(peer, CreateCommand(placement.PlacementId, placement.Position), out _);

            Assert.That(count, Is.EqualTo(0));
            Assert.That(overlayStore.TryGetResource(placement.PlacementId, out var state), Is.True);
            Assert.That(state.RemainingAmount, Is.EqualTo(0));
            Assert.That(state.Flags, Is.EqualTo(OpenWorldResourceOverlayFlags.Depleted));
        }

        [Test]
        public void ServerCommand_OutOfRangePlayer_IsRejectedWithoutMutation()
        {
            using var scope = new OpenWorldResourcesHarvestWorldScope(createServer: true, createClient: false);
            var peer = new NetworkPeerId(10);
            scope.CreateServerPlayer(peer, new Vector3(20f, 0f, 0f));
            var placement = CreatePlacement(103, new Vector3(2f, 0f, 0f));
            scope.RegisterServerPlacement(placement);

            var count = RunServerCommand(peer, CreateCommand(placement.PlacementId, placement.Position), out _);

            Assert.That(count, Is.EqualTo(0));
            Assert.That(SW.GetResource<OpenWorldChunkOverlayStore>().TryGetResource(placement.PlacementId, out _), Is.False);
        }

        [Test]
        public void ServerCommand_NonDefaultTool_IsRejectedWithoutMutation()
        {
            using var scope = new OpenWorldResourcesHarvestWorldScope(createServer: true, createClient: false);
            var peer = new NetworkPeerId(11);
            scope.CreateServerPlayer(peer, Vector3.zero);
            var placement = CreatePlacement(104, new Vector3(2f, 0f, 0f));
            scope.RegisterServerPlacement(placement);
            var command = CreateCommand(placement.PlacementId, placement.Position);
            command.ToolId = 1;

            var count = RunServerCommand(peer, command, out _);

            Assert.That(count, Is.EqualTo(0));
            Assert.That(SW.GetResource<OpenWorldChunkOverlayStore>().TryGetResource(placement.PlacementId, out _), Is.False);
        }

        [Test]
        public void ServerCommand_OutOfRangeHitPoint_IsRejectedWithoutMutation()
        {
            using var scope = new OpenWorldResourcesHarvestWorldScope(createServer: true, createClient: false);
            var peer = new NetworkPeerId(12);
            scope.CreateServerPlayer(peer, Vector3.zero);
            var placement = CreatePlacement(105, new Vector3(2f, 0f, 0f));
            scope.RegisterServerPlacement(placement);

            var count = RunServerCommand(peer, CreateCommand(placement.PlacementId, new Vector3(20f, 0f, 0f)), out _);

            Assert.That(count, Is.EqualTo(0));
            Assert.That(SW.GetResource<OpenWorldChunkOverlayStore>().TryGetResource(placement.PlacementId, out _), Is.False);
        }

        [Test]
        public void ClientInput_PrimaryPress_SendsHarvestCommandForNearestTarget()
        {
            using var scope = new OpenWorldResourcesHarvestWorldScope(createServer: true, createClient: true);
            scope.CreateClientPlayer(Vector3.zero);
            var placement = CreatePlacement(201, new Vector3(2f, 0f, 0f));
            scope.CreateClientResourceTarget(placement);
            scope.ConfigurePrimaryPressed();

            var receiver = SW.RegisterEventReceiver<NetworkEventFromClient<TryHarvestOpenWorldResourceCommand>>();
            var sendSystem = new ClientNetworkEventSendSystem();
            sendSystem.Init();
            try
            {
                new ClientOpenWorldResourceHarvestInputSystem().Update();
                sendSystem.Update();
                CW.GetResource<NetOutbox>().FlushNetworkEventBatches();

                Assert.That(CW.GetResource<NetOutbox>().Packets.Count, Is.EqualTo(1));
                var packet = CW.GetResource<NetOutbox>().Packets[0];
                Assert.That(packet.Delivery, Is.EqualTo(NetDelivery.ReliableSequenced));
                Assert.That(PacketCodec.Decode(new NetworkPeerId(1), packet.Payload, SW.GetResource<NetInbox>()), Is.True);
                new ServerNetworkEventApplySystem().Update();

                var count = 0;
                foreach (var evt in receiver)
                {
                    count++;
                    Assert.That(evt.Value.SourcePeer, Is.EqualTo(new NetworkPeerId(1)));
                    Assert.That(evt.Value.Value.PlacementId, Is.EqualTo(placement.PlacementId));
                    Assert.That(evt.Value.Value.ToolId, Is.EqualTo(0));
                    Assert.That(evt.Value.Value.HitPointXQ, Is.EqualTo(200));
                    Assert.That(evt.Value.Value.HitPointYQ, Is.EqualTo(0));
                    Assert.That(evt.Value.Value.HitPointZQ, Is.EqualTo(0));
                }

                Assert.That(count, Is.EqualTo(1));
            }
            finally
            {
                sendSystem.Destroy();
                SW.DeleteEventReceiver(ref receiver);
            }
        }

        private static int RunServerCommand(
            NetworkPeerId peer,
            TryHarvestOpenWorldResourceCommand command,
            out OpenWorldResourceHarvestedEvent harvested)
        {
            var receiver = SW.RegisterEventReceiver<OpenWorldResourceHarvestedEvent>();
            var system = new ServerOpenWorldResourceHarvestCommandSystem();
            harvested = default;
            var count = 0;

            system.Init();
            try
            {
                SW.SendEvent(new NetworkEventFromClient<TryHarvestOpenWorldResourceCommand>(peer, command));
                system.Update();

                foreach (var evt in receiver)
                {
                    harvested = evt.Value;
                    count++;
                }
            }
            finally
            {
                system.Destroy();
                SW.DeleteEventReceiver(ref receiver);
            }

            return count;
        }

        private static ResourcePlacement CreatePlacement(long placementId, Vector3 position, ushort kindId = 2)
        {
            return new ResourcePlacement(
                placementId,
                new ResourcePlacementKindId(kindId),
                new WorldChunkId(1, 2),
                position,
                0f,
                1f);
        }

        private static TryHarvestOpenWorldResourceCommand CreateCommand(long placementId, Vector3 hitPoint)
        {
            return new TryHarvestOpenWorldResourceCommand
            {
                PlacementId = placementId,
                ToolId = 0,
                HitPointXQ = Mathf.RoundToInt(hitPoint.x / 0.01f),
                HitPointYQ = Mathf.RoundToInt(hitPoint.y / 0.01f),
                HitPointZQ = Mathf.RoundToInt(hitPoint.z / 0.01f)
            };
        }

        private static int CountDamageEffectsFor(EntityGID targetGid)
        {
            var count = 0;
            foreach (var effect in SW.Query<All<DamageEffectTag, EffectTarget>>().Entities())
            {
                if (effect.Read<EffectTarget>().Value.Equals(targetGid))
                    count++;
            }

            return count;
        }

        private sealed class OpenWorldResourcesHarvestWorldScope : IDisposable
        {
            public OpenWorldResourcesHarvestWorldScope(bool createServer, bool createClient)
            {
                if (SW.Status != WorldStatus.NotCreated)
                    SW.Destroy();
                if (CW.Status != WorldStatus.NotCreated)
                    CW.Destroy();

                NetworkRuntime.LocalPeerId = new NetworkPeerId(1);
                NetworkEventRegistry.Clear();
                new OpenWorldResourcesGameplayFeature().RegisterNetworkEvents();

                if (createClient)
                    CreateClientWorld();
                if (createServer)
                    CreateServerWorld();
            }

            public void CreateServerPlayer(NetworkPeerId owner, Vector3 position)
            {
                var player = SW.NewEntity<Default>();
                player.Set<PlayerTag>();
                player.Set(new NetworkIdentity
                {
                    Owner = owner,
                    Authority = NetworkAuthority.Owner,
                    NetworkArchetypeId = 0
                });
                player.Set(new CharacterNetState
                {
                    Position = position,
                    Rotation = Quaternion.identity
                });
            }

            public SW.Entity CreateServerCharacterWithHealth(Vector3 position, float current, float max)
            {
                var entity = SW.NewEntity<Default>();
                entity.Set(new CharacterNetState
                {
                    Position = position,
                    Rotation = Quaternion.identity
                });
                entity.Set(new Health
                {
                    Current = current,
                    Max = max
                });
                return entity;
            }

            public void AdvanceSimulationSeconds(float seconds)
            {
                var simulationTime = SW.GetResource<SimulationTime>();
                var ticks = simulationTime.SecondsToTicks(seconds);
                simulationTime.ServerTick += ticks;
                simulationTime.ElapsedSeconds += ticks * simulationTime.FixedStepSeconds;
            }

            public void RegisterServerPlacement(ResourcePlacement placement)
            {
                SW.GetResource<OpenWorldPlacementIndexStore>().RegisterChunkPlacements(
                    placement.ChunkId,
                    new[] { placement });
                SW.GetResource<OpenWorldChunkOverlayStore>().RegisterPlacement(
                    placement.ChunkId,
                    placement.PlacementId);
            }

            public void CreateClientPlayer(Vector3 position)
            {
                var player = CW.NewEntity<Default>();
                player.Set<LocalOwned>();
                player.Set<PlayerTag>();
                player.Set(new CharacterNetState
                {
                    Position = position,
                    Rotation = Quaternion.identity
                });
            }

            public void CreateClientResourceTarget(ResourcePlacement placement)
            {
                var target = CW.NewEntity<Default>();
                target.Set<OpenWorldResourceTargetable>();
                target.Set(new OpenWorldResourceTargetState
                {
                    PlacementId = placement.PlacementId,
                    ChunkId = placement.ChunkId,
                    KindIdValue = placement.KindId.Value,
                    RemainingAmount = 8,
                    Flags = OpenWorldResourceOverlayFlags.None,
                    WorldPosition = placement.Position
                });
            }

            public void ConfigurePrimaryPressed()
            {
                var inputState = CW.GetResource<ClientInputState>();
                var aimRay = default(Ray);
                inputState.Configure(
                    0,
                    new[] { CoreInputActions.Primary },
                    new[] { true });
                inputState.BeginFrame(Vector2.zero, false, in aimRay);
                inputState.UpdateButton(0, true);
            }

            public void Dispose()
            {
                if (CW.Status != WorldStatus.NotCreated)
                    CW.Destroy();
                if (SW.Status != WorldStatus.NotCreated)
                    SW.Destroy();
                NetworkEventRegistry.Clear();
            }

            private static void CreateClientWorld()
            {
                CW.Create(WorldConfig.Default());
                CW.Types().RegisterAll(
                    typeof(ClientCoreWT).Assembly,
                    typeof(CharacterNetState).Assembly,
                    typeof(OpenWorldResourceTargetState).Assembly,
                    typeof(OpenWorldResourcesGameplayFeature).Assembly);
                NetworkEventRegistry.RegisterClientWorldTypes();
                CW.Initialize();

                CW.SetResource(new ClientInputState());
                CW.SetResource(new NetOutbox());
            }

            private static void CreateServerWorld()
            {
                SW.Create(WorldConfig.Default());
                SW.Types().RegisterAll(
                    typeof(ServerWT).Assembly,
                    typeof(CharacterNetState).Assembly,
                    typeof(EffectTag).Assembly,
                    typeof(Health).Assembly,
                    typeof(OpenWorldResourceTargetState).Assembly,
                    typeof(OpenWorldResourcesGameplayFeature).Assembly);
                NetworkEventRegistry.RegisterServerWorldTypes();
                SW.Initialize();

                var dirtyQueue = new OpenWorldChunkOverlayDirtyQueue();
                SW.SetResource(new NetInbox());
                SW.SetResource(new SimulationTime
                {
                    FixedStepSeconds = 1f / 30f
                });
                SW.SetResource(new CombatDebugLogBuffer());
                SW.SetResource(new OpenWorldPlacementIndexStore());
                SW.SetResource(new OpenWorldChunkOverlayStore(dirtyQueue));
                SW.SetResource(dirtyQueue);
            }
        }
    }
}
