using System;
using NUnit.Framework;
using StaticMlp.Features.Combat;
using StaticMlp.Features.CombatDirector;
using StaticMlp.Features.OpenWorldResources;
using StaticMlp.Features.ResourcesInventoryMinimal;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Replication.Generated;
using UnityEngine;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Tests.CombatDirector
{
    public sealed class CombatDirectorRuntimeSystemsTests
    {
        [Test]
        public void CombatCellTrackingSystem_SinglePlayer_CentersCellOnPlayer()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            var player = scope.CreatePlayer(new NetworkPeerId(7), new Vector3(3f, 0f, 9f));
            var system = new CombatCellTrackingSystem();

            system.Update();

            var cell = scope.ReadSingleCombatCell();
            Assert.That(cell.Center.x, Is.EqualTo(player.Read<CharacterNetState>().Position.x).Within(0.001f));
            Assert.That(cell.Center.y, Is.EqualTo(player.Read<CharacterNetState>().Position.y).Within(0.001f));
            Assert.That(cell.Center.z, Is.EqualTo(player.Read<CharacterNetState>().Position.z).Within(0.001f));
        }

        [Test]
        public void CombatCellTrackingSystem_MultipleNearbyPlayers_AveragesActiveGroup()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(1), new Vector3(0f, 0f, 0f));
            scope.CreatePlayer(new NetworkPeerId(2), new Vector3(10f, 0f, 4f));
            var system = new CombatCellTrackingSystem();

            system.Update();

            var cell = scope.ReadSingleCombatCell();
            Assert.That(cell.Center.x, Is.EqualTo(5f).Within(0.001f));
            Assert.That(cell.Center.y, Is.EqualTo(0f).Within(0.001f));
            Assert.That(cell.Center.z, Is.EqualTo(2f).Within(0.001f));
        }

        [Test]
        public void PlayerThreatInputSystem_AttackAndHarvestInputs_AccumulateIntoPlayerNoise()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            var peer = new NetworkPeerId(9);
            var player = scope.CreatePlayer(peer, Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var threatInputSystem = new PlayerThreatInputSystem();

            threatInputSystem.Init();
            try
            {
                cellTrackingSystem.Update();
                threatInputSystem.Update();
                var baselineNoise = player.Read<PlayerNoise>().Value;

                ref var attackState = ref player.Mut<ServerCombatAttackState>();
                attackState.LastAcceptedShotSequence += 2;
                SW.SendEvent(new NetworkEventFromClient<TryHarvestOpenWorldResourceCommand>(peer, default));

                threatInputSystem.Update();

                Assert.That(player.Read<PlayerNoise>().Value, Is.GreaterThan(baselineNoise + 2.5f));
            }
            finally
            {
                threatInputSystem.Destroy();
            }
        }

        [Test]
        public void PlayerThreatInputSystem_ResourcesInventory_MapsIntoCarriedLootValue()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            var player = scope.CreatePlayer(new NetworkPeerId(11), Vector3.zero);
            player.Set(new ResourcesInventory
            {
                Wood = 7,
                Stone = 5
            });

            var cellTrackingSystem = new CombatCellTrackingSystem();
            var threatInputSystem = new PlayerThreatInputSystem();

            threatInputSystem.Init();
            try
            {
                cellTrackingSystem.Update();
                threatInputSystem.Update();

                Assert.That(player.Read<CarriedLootValue>().Value, Is.EqualTo(12f));
            }
            finally
            {
                threatInputSystem.Destroy();
            }
        }

        private sealed class CombatDirectorTestServerWorldScope : IDisposable
        {
            public CombatDirectorTestServerWorldScope()
            {
                if (SW.Status != WorldStatus.NotCreated)
                    SW.Destroy();

                NetworkEventRegistry.Clear();
                ReplicatedNetworkEventRegistry.RegisterNetworkEvents();
                ReplicatedComponentRegistration.RegisterReplicationComponents();
                new OpenWorldResourcesGameplayFeature().RegisterNetworkEvents();
                SW.Create(WorldConfig.Default());
                SW.Types().RegisterAll(
                    typeof(ServerWT).Assembly,
                    typeof(PlayerTag).Assembly,
                    typeof(ServerCombatAttackState).Assembly,
                    typeof(OpenWorldResourcesGameplayFeature).Assembly,
                    typeof(ResourcesInventory).Assembly,
                    typeof(CombatDirectorGameplayFeature).Assembly);
                NetworkEventRegistry.RegisterServerWorldTypes();
                SW.Initialize();
                SW.SetResource(new SimulationTime
                {
                    FixedStepSeconds = 0.5f
                });
                SW.SetResource(EncounterDirectorConfig.CreateDefault());
                SW.SetResource(EnemySpawnCatalog.CreateDefault());
            }

            public SW.Entity CreatePlayer(NetworkPeerId owner, Vector3 position)
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
                player.Set(new ServerCombatAttackState());
                return player;
            }

            public CombatCell ReadSingleCombatCell()
            {
                var found = false;
                CombatCell cell = default;
                foreach (var entity in SW.Query<All<CombatCell>>().Entities())
                {
                    if (found)
                        throw new InvalidOperationException("Expected exactly one combat cell in test scope.");

                    cell = entity.Read<CombatCell>();
                    found = true;
                }

                if (!found)
                    throw new InvalidOperationException("Combat cell was not created by the tracking system.");

                return cell;
            }

            public void Dispose()
            {
                NetworkEventRegistry.Clear();
                if (SW.Status != WorldStatus.NotCreated)
                    SW.Destroy();
            }
        }
    }
}
