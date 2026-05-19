using System;
using NUnit.Framework;
using StaticMlp.Features.AiBots;
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

        [Test]
        public void ThreatBudgetAccumulationSystem_PlayerNoiseAndLoot_AccumulatesBudget()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            var player = scope.CreatePlayer(new NetworkPeerId(15), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var threatInputSystem = new PlayerThreatInputSystem();
            var budgetSystem = new ThreatBudgetAccumulationSystem();

            threatInputSystem.Init();
            try
            {
                cellTrackingSystem.Update();
                threatInputSystem.Update();

                player.Mut<PlayerNoise>().Value = 4f;
                player.Mut<CarriedLootValue>().Value = 3f;

                budgetSystem.Update();

                var budget = scope.ReadThreatBudget();
                Assert.That(budget.AccumulationPerSecond, Is.EqualTo(12f).Within(0.001f));
                Assert.That(budget.Current, Is.EqualTo(6f).Within(0.001f));
            }
            finally
            {
                threatInputSystem.Destroy();
            }
        }

        [Test]
        public void ThreatBudgetAccumulationSystem_ClampPreventsOverflow()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            var player = scope.CreatePlayer(new NetworkPeerId(17), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var threatInputSystem = new PlayerThreatInputSystem();
            var budgetSystem = new ThreatBudgetAccumulationSystem();

            threatInputSystem.Init();
            try
            {
                cellTrackingSystem.Update();
                threatInputSystem.Update();
                player.Mut<PlayerNoise>().Value = 100f;
                player.Mut<CarriedLootValue>().Value = 100f;

                var directorEntity = scope.GetDirectorEntity();
                ref var budget = ref directorEntity.Mut<ThreatBudget>();
                budget.Current = budget.Max - 0.25f;

                budgetSystem.Update();

                Assert.That(scope.ReadThreatBudget().Current, Is.EqualTo(budget.Max).Within(0.001f));
            }
            finally
            {
                threatInputSystem.Destroy();
            }
        }

        [Test]
        public void DirectorPhaseSystem_ThresholdsAndSpawnSource_TransitionDeterministically()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(21), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var phaseSystem = new DirectorPhaseSystem();

            cellTrackingSystem.Update();

            var directorEntity = scope.GetDirectorEntity();
            directorEntity.Mut<ThreatBudget>().Current = 31f;

            phaseSystem.Update();

            Assert.That(scope.ReadDirectorState().Phase, Is.EqualTo(DirectorPhase.BuildUp));

            scope.CreateSpawnSource(new Vector3(20f, 0f, 0f), isActive: true);
            directorEntity.Mut<ThreatBudget>().Current = 76f;

            phaseSystem.Update();

            var peakState = scope.ReadDirectorState();
            Assert.That(peakState.Phase, Is.EqualTo(DirectorPhase.Peak));
            Assert.That(peakState.PhaseTimer, Is.EqualTo(0f).Within(0.001f));
            Assert.That(peakState.TimeSinceLastPeak, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void DirectorPhaseSystem_ReliefAndCooldown_PreventImmediateSecondPeak()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(23), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var phaseSystem = new DirectorPhaseSystem();

            cellTrackingSystem.Update();
            scope.CreateSpawnSource(new Vector3(20f, 0f, 0f), isActive: true);

            var directorEntity = scope.GetDirectorEntity();
            ref var state = ref directorEntity.Mut<DirectorState>();
            ref var budget = ref directorEntity.Mut<ThreatBudget>();
            state.Phase = DirectorPhase.Peak;
            state.PhaseTimer = 0f;
            state.TimeSinceLastPeak = 0f;
            budget.Current = 76f;

            phaseSystem.Update();
            Assert.That(scope.ReadDirectorState().Phase, Is.EqualTo(DirectorPhase.Relief));

            state = directorEntity.Mut<DirectorState>();
            state.PhaseTimer = 19f;
            phaseSystem.Update();
            Assert.That(scope.ReadDirectorState().Phase, Is.EqualTo(DirectorPhase.Relief));

            phaseSystem.Update();
            Assert.That(scope.ReadDirectorState().Phase, Is.EqualTo(DirectorPhase.Cooldown));

            state = directorEntity.Mut<DirectorState>();
            state.PhaseTimer = 14f;
            phaseSystem.Update();
            Assert.That(scope.ReadDirectorState().Phase, Is.EqualTo(DirectorPhase.Cooldown));

            phaseSystem.Update();
            Assert.That(scope.ReadDirectorState().Phase, Is.EqualTo(DirectorPhase.Calm));
        }

        [Test]
        public void DirectorSystems_IdenticalInputState_ProducesDeterministicResults()
        {
            var first = RunDeterministicDirectorSequence();
            var second = RunDeterministicDirectorSequence();

            Assert.That(second.Phase, Is.EqualTo(first.Phase));
            Assert.That(second.PhaseTimer, Is.EqualTo(first.PhaseTimer).Within(0.001f));
            Assert.That(second.TimeSinceLastPeak, Is.EqualTo(first.TimeSinceLastPeak).Within(0.001f));
            Assert.That(second.BudgetCurrent, Is.EqualTo(first.BudgetCurrent).Within(0.001f));
            Assert.That(second.BudgetAccumulationPerSecond, Is.EqualTo(first.BudgetAccumulationPerSecond).Within(0.001f));
        }

        [Test]
        public void SpawnSourceSelectionSystem_InactiveOrInvalidDistanceSources_AreRejected()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(31), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var selectionSystem = new SpawnSourceSelectionSystem();

            cellTrackingSystem.Update();
            var directorEntity = scope.GetDirectorEntity();
            directorEntity.Mut<DirectorState>().Phase = DirectorPhase.BuildUp;

            scope.CreateSpawnSource(new Vector3(20f, 0f, 0f), isActive: false);
            selectionSystem.Update();
            Assert.That(directorEntity.Has<SelectedSpawnSource>(), Is.False);

            scope.CreateSpawnSource(new Vector3(2f, 0f, 0f), isActive: true);
            selectionSystem.Update();
            Assert.That(directorEntity.Has<SelectedSpawnSource>(), Is.False);

            scope.CreateSpawnSource(new Vector3(120f, 0f, 0f), isActive: true);
            selectionSystem.Update();
            Assert.That(directorEntity.Has<SelectedSpawnSource>(), Is.False);
        }

        [Test]
        public void SpawnRequestBuildSystem_LowMediumHighBudgets_CreateExpectedRoleCounts()
        {
            var low = BuildRequestsForBudget(31f, DirectorPhase.BuildUp);
            Assert.That(low.Swarmers, Is.EqualTo(10));
            Assert.That(low.Markers, Is.EqualTo(0));
            Assert.That(low.Anchors, Is.EqualTo(0));

            var medium = BuildRequestsForBudget(60f, DirectorPhase.BuildUp);
            Assert.That(medium.Swarmers, Is.EqualTo(14));
            Assert.That(medium.Markers, Is.EqualTo(1));
            Assert.That(medium.Anchors, Is.EqualTo(0));

            var high = BuildRequestsForBudget(80f, DirectorPhase.Peak);
            Assert.That(high.Swarmers, Is.EqualTo(18));
            Assert.That(high.Markers, Is.EqualTo(1));
            Assert.That(high.Anchors, Is.EqualTo(1));
        }

        [Test]
        public void SpawnRequestBuildSystem_SpawnCap_PreventsOverSpawnRequests()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(33), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var selectionSystem = new SpawnSourceSelectionSystem();
            var buildSystem = new SpawnRequestBuildSystem();

            cellTrackingSystem.Update();
            for (var i = 0; i < 22; i++)
                scope.CreateEnemy(new Vector3(i * 0.1f, 0f, 0f), EnemyRole.Swarmer);

            scope.CreateSpawnSource(new Vector3(20f, 0f, 0f), isActive: true);
            var directorEntity = scope.GetDirectorEntity();
            directorEntity.Mut<DirectorState>().Phase = DirectorPhase.Peak;
            directorEntity.Mut<ThreatBudget>().Current = 80f;

            selectionSystem.Update();
            buildSystem.Update();

            Assert.That(scope.CountSpawnRequestEnemies(), Is.LessThanOrEqualTo(2));
        }

        [Test]
        public void SpawnRequestValidationSystem_InvalidCatalogRole_RejectsRequest()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(35), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var validationSystem = new SpawnRequestValidationSystem();

            cellTrackingSystem.Update();
            var directorEntity = scope.GetDirectorEntity();
            directorEntity.Mut<DirectorState>().Phase = DirectorPhase.BuildUp;
            directorEntity.Mut<ThreatBudget>().Current = 20f;
            var source = scope.CreateSpawnSource(new Vector3(20f, 0f, 0f), isActive: true);
            var request = scope.CreateSpawnRequest(source, (EnemyRole)250, count: 1);
            var requestGid = request.GID;

            validationSystem.Update();

            Assert.That(requestGid.TryUnpack<ServerWT>(out _), Is.False);
        }

        [Test]
        public void EnemySpawnApplySystem_ValidRequest_CreatesEnemiesConsumesBudgetAndEmitsEvent()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(37), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var validationSystem = new SpawnRequestValidationSystem();
            var applySystem = new EnemySpawnApplySystem();

            cellTrackingSystem.Update();
            var directorEntity = scope.GetDirectorEntity();
            directorEntity.Mut<DirectorState>().Phase = DirectorPhase.BuildUp;
            directorEntity.Mut<ThreatBudget>().Current = 20f;
            var source = scope.CreateSpawnSource(new Vector3(20f, 0f, 0f), isActive: true);
            scope.CreateSpawnRequest(source, EnemyRole.Swarmer, count: 2);

            var receiver = SW.RegisterEventReceiver<EnemySpawnedEvent>();
            try
            {
                validationSystem.Update();
                applySystem.Update();

                var spawnedEvents = 0;
                foreach (var evt in receiver)
                {
                    Assert.That(evt.Value.SpawnedEntity.TryUnpack<ServerWT>(out _), Is.True);
                    Assert.That(evt.Value.Role, Is.EqualTo(EnemyRole.Swarmer));
                    Assert.That(evt.Value.SourceType, Is.EqualTo(SpawnSourceType.Burrow));
                    spawnedEvents++;
                }

                Assert.That(spawnedEvents, Is.EqualTo(2));
                Assert.That(scope.CountEnemies(EnemyRole.Swarmer), Is.EqualTo(2));
                Assert.That(scope.ReadThreatBudget().Current, Is.EqualTo(18f).Within(0.001f));
            }
            finally
            {
                SW.DeleteEventReceiver(ref receiver);
            }
        }

        private static DeterministicDirectorResult RunDeterministicDirectorSequence()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            var player = scope.CreatePlayer(new NetworkPeerId(29), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var threatInputSystem = new PlayerThreatInputSystem();
            var budgetSystem = new ThreatBudgetAccumulationSystem();
            var phaseSystem = new DirectorPhaseSystem();

            threatInputSystem.Init();
            try
            {
                cellTrackingSystem.Update();
                threatInputSystem.Update();
                scope.CreateSpawnSource(new Vector3(20f, 0f, 0f), isActive: true);

                player.Mut<PlayerNoise>().Value = 8f;
                player.Mut<CarriedLootValue>().Value = 10f;

                for (var i = 0; i < 15; i++)
                {
                    budgetSystem.Update();
                    phaseSystem.Update();
                }

                var state = scope.ReadDirectorState();
                var budget = scope.ReadThreatBudget();
                return new DeterministicDirectorResult(
                    state.Phase,
                    state.PhaseTimer,
                    state.TimeSinceLastPeak,
                    budget.Current,
                    budget.AccumulationPerSecond);
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
                    typeof(CombatCell).Assembly,
                    typeof(ReplicatedComponentRegistration).Assembly,
                    typeof(ServerWT).Assembly,
                    typeof(PlayerTag).Assembly,
                    typeof(ServerCombatAttackState).Assembly,
                    typeof(AiBotsGameplayFeature).Assembly,
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
                SW.SetResource(new AiBotFactory());
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

            public SW.Entity CreateSpawnSource(Vector3 position, bool isActive)
            {
                var source = SW.NewEntity<Default>();
                source.Set(new SpawnSource
                {
                    Type = SpawnSourceType.Burrow,
                    Position = new Unity.Mathematics.float3(position.x, position.y, position.z),
                    Radius = 5f,
                    IsActive = isActive
                });
                return source;
            }

            public SW.Entity CreateEnemy(Vector3 position, EnemyRole role)
            {
                var enemy = SW.NewEntity<Default>();
                enemy.Set<EnemyTag>();
                enemy.Set(new EnemyArchetype
                {
                    Role = role
                });
                enemy.Set(new CharacterNetState
                {
                    Position = position,
                    Rotation = Quaternion.identity
                });
                return enemy;
            }

            public SW.Entity CreateSpawnRequest(SW.Entity source, EnemyRole role, int count)
            {
                ref readonly var spawnSource = ref source.Read<SpawnSource>();
                var request = SW.NewEntity<Default>();
                request.Set(new SpawnRequest
                {
                    CellId = ReadSingleCombatCell().CellId,
                    SourceEntity = source.GID,
                    SourceType = spawnSource.Type,
                    Role = role,
                    Count = count,
                    SpawnPosition = spawnSource.Position
                });
                return request;
            }

            public int CountSpawnRequestEnemies()
            {
                var count = 0;
                foreach (var request in SW.Query<All<SpawnRequest>>().Entities())
                    count += request.Read<SpawnRequest>().Count;

                return count;
            }

            public int CountEnemies(EnemyRole role)
            {
                var count = 0;
                foreach (var enemy in SW.Query<All<EnemyTag, EnemyArchetype>>().Entities())
                {
                    if (enemy.Read<EnemyArchetype>().Role == role)
                        count++;
                }

                return count;
            }

            public SW.Entity GetDirectorEntity()
            {
                var found = false;
                SW.Entity directorEntity = default;
                foreach (var entity in SW.Query<All<CombatCell, ThreatBudget, DirectorState>>().Entities())
                {
                    if (found)
                        throw new InvalidOperationException("Expected exactly one director entity in test scope.");

                    directorEntity = entity;
                    found = true;
                }

                if (!found)
                    throw new InvalidOperationException("Director entity was not created by the tracking system.");

                return directorEntity;
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

            public ThreatBudget ReadThreatBudget()
            {
                return GetDirectorEntity().Read<ThreatBudget>();
            }

            public DirectorState ReadDirectorState()
            {
                return GetDirectorEntity().Read<DirectorState>();
            }

            public void Dispose()
            {
                NetworkEventRegistry.Clear();
                if (SW.Status != WorldStatus.NotCreated)
                    SW.Destroy();
            }
        }

        private readonly struct DeterministicDirectorResult
        {
            public readonly DirectorPhase Phase;
            public readonly float PhaseTimer;
            public readonly float TimeSinceLastPeak;
            public readonly float BudgetCurrent;
            public readonly float BudgetAccumulationPerSecond;

            public DeterministicDirectorResult(
                DirectorPhase phase,
                float phaseTimer,
                float timeSinceLastPeak,
                float budgetCurrent,
                float budgetAccumulationPerSecond)
            {
                Phase = phase;
                PhaseTimer = phaseTimer;
                TimeSinceLastPeak = timeSinceLastPeak;
                BudgetCurrent = budgetCurrent;
                BudgetAccumulationPerSecond = budgetAccumulationPerSecond;
            }
        }

        private static SpawnRequestRoleCounts BuildRequestsForBudget(float budgetCurrent, DirectorPhase phase)
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(41), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var selectionSystem = new SpawnSourceSelectionSystem();
            var buildSystem = new SpawnRequestBuildSystem();

            cellTrackingSystem.Update();
            scope.CreateSpawnSource(new Vector3(20f, 0f, 0f), isActive: true);
            var directorEntity = scope.GetDirectorEntity();
            directorEntity.Mut<DirectorState>().Phase = phase;
            directorEntity.Mut<ThreatBudget>().Current = budgetCurrent;

            selectionSystem.Update();
            buildSystem.Update();

            var counts = new SpawnRequestRoleCounts();
            foreach (var request in SW.Query<All<SpawnRequest>>().Entities())
            {
                ref readonly var spawnRequest = ref request.Read<SpawnRequest>();
                switch (spawnRequest.Role)
                {
                    case EnemyRole.Swarmer:
                        counts.Swarmers += spawnRequest.Count;
                        break;
                    case EnemyRole.Marker:
                        counts.Markers += spawnRequest.Count;
                        break;
                    case EnemyRole.AnchorElite:
                        counts.Anchors += spawnRequest.Count;
                        break;
                    default:
                        throw new InvalidOperationException($"Unexpected enemy role in spawn request: {spawnRequest.Role}.");
                }
            }

            return counts;
        }

        private struct SpawnRequestRoleCounts
        {
            public int Swarmers;
            public int Markers;
            public int Anchors;
        }
    }
}
