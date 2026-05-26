using System;
using NUnit.Framework;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Combat;
using StaticMlp.Features.CombatDirector;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Features.OpenWorldResources;
using StaticMlp.Features.ResourcesInventoryMinimal;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Replication.Generated;
using UnityEngine;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Shared;

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
        public void PlayerThreatInputSystem_AttackAndHarvestInputs_MapIntoExplicitPlayerCauses()
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

                Assert.That(player.Read<PlayerNoise>().Value, Is.EqualTo(baselineNoise + 1f).Within(0.001f));
                Assert.That(player.Read<PlayerCombatAttention>().Value, Is.EqualTo(2f).Within(0.001f));
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
        public void CellAttentionInputSystem_PassivePlayerPresence_StaysCalm()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(13), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var threatInputSystem = new PlayerThreatInputSystem();
            var attentionInputSystem = new CellAttentionInputSystem();

            threatInputSystem.Init();
            try
            {
                cellTrackingSystem.Update();
                threatInputSystem.Update();
                attentionInputSystem.Update();

                Assert.That(scope.ReadCellAttention().Current, Is.EqualTo(0f).Within(0.001f));
                Assert.That(scope.ReadThreatBudget().Current, Is.EqualTo(0f).Within(0.001f));
            }
            finally
            {
                threatInputSystem.Destroy();
            }
        }

        [Test]
        public void CellAttentionInputSystem_PlayerNoiseCombatAndLoot_AccumulateNamedAttention()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            var player = scope.CreatePlayer(new NetworkPeerId(15), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var threatInputSystem = new PlayerThreatInputSystem();
            var attentionInputSystem = new CellAttentionInputSystem();

            threatInputSystem.Init();
            try
            {
                cellTrackingSystem.Update();
                threatInputSystem.Update();
                player.Mut<PlayerNoise>().Value = 4f;
                player.Mut<PlayerCombatAttention>().Value = 2f;
                player.Mut<CarriedLootValue>().Value = 3f;

                attentionInputSystem.Update();

                var attention = scope.ReadCellAttention();
                Assert.That(attention.Noise, Is.EqualTo(5f).Within(0.001f));
                Assert.That(attention.Combat, Is.EqualTo(6f).Within(0.001f));
                Assert.That(attention.Loot, Is.EqualTo(2.25f).Within(0.001f));
                Assert.That(attention.Current, Is.EqualTo(13.25f).Within(0.001f));
                var budget = scope.ReadThreatBudget();
                Assert.That(budget.AccumulationPerSecond, Is.EqualTo(26.5f).Within(0.001f));
                Assert.That(budget.Current, Is.EqualTo(attention.Current).Within(0.001f));
            }
            finally
            {
                threatInputSystem.Destroy();
            }
        }

        [Test]
        public void CellAttentionInputSystem_ClampPreventsOverflow()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            var player = scope.CreatePlayer(new NetworkPeerId(17), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var threatInputSystem = new PlayerThreatInputSystem();
            var attentionInputSystem = new CellAttentionInputSystem();

            threatInputSystem.Init();
            try
            {
                cellTrackingSystem.Update();
                threatInputSystem.Update();
                player.Mut<PlayerNoise>().Value = 100f;
                player.Mut<CarriedLootValue>().Value = 100f;

                var directorEntity = scope.GetDirectorEntity();
                ref var attention = ref directorEntity.Mut<CellAttention>();
                attention.Noise = attention.Max - 0.25f;

                attentionInputSystem.Update();

                Assert.That(scope.ReadCellAttention().Current, Is.EqualTo(attention.Max).Within(0.001f));
                Assert.That(scope.ReadThreatBudget().Current, Is.EqualTo(attention.Max).Within(0.001f));
            }
            finally
            {
                threatInputSystem.Destroy();
            }
        }

        [Test]
        public void CellAttentionDecaySystem_DecaysAttentionBackTowardCalm()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(19), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var decaySystem = new CellAttentionDecaySystem();

            cellTrackingSystem.Update();
            var directorEntity = scope.GetDirectorEntity();
            ref var attention = ref directorEntity.Mut<CellAttention>();
            attention.Noise = 5f;
            attention.Combat = 4f;
            attention.Loot = 3f;
            attention.Current = 12f;

            decaySystem.Update();

            var decayedAttention = scope.ReadCellAttention();
            Assert.That(decayedAttention.Noise, Is.EqualTo(4.791666f).Within(0.001f));
            Assert.That(decayedAttention.Combat, Is.EqualTo(3.833333f).Within(0.001f));
            Assert.That(decayedAttention.Loot, Is.EqualTo(2.875f).Within(0.001f));
            Assert.That(decayedAttention.Current, Is.EqualTo(11.5f).Within(0.001f));
        }

        [Test]
        public void DirectorPhaseSystem_PassiveExploration_RemainsAmbient()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(21), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var phaseSystem = new DirectorPhaseSystem();

            cellTrackingSystem.Update();
            phaseSystem.Update();

            var state = scope.ReadDirectorState();
            Assert.That(state.Phase, Is.EqualTo(DirectorPhase.Ambient));
            Assert.That(state.PhaseTimer, Is.EqualTo(0.5f).Within(0.001f));
        }

        [Test]
        public void DirectorPhaseSystem_HighAttentionAlone_CannotCreatePressureEvent()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(23), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var phaseSystem = new DirectorPhaseSystem();

            cellTrackingSystem.Update();
            var directorEntity = scope.GetDirectorEntity();
            ref var attention = ref directorEntity.Mut<CellAttention>();
            attention.Noise = 80f;
            attention.Current = 80f;

            phaseSystem.Update();

            Assert.That(scope.ReadDirectorState().Phase, Is.EqualTo(DirectorPhase.Suspicion));
            Assert.That(scope.CountSpawnRequestEnemies(), Is.EqualTo(0));
        }

        [Test]
        public void DirectorPhaseSystem_SuspicionCanDecayToRecoveryAndCooldown()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(24), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var phaseSystem = new DirectorPhaseSystem();

            cellTrackingSystem.Update();
            var directorEntity = scope.GetDirectorEntity();
            ref var state = ref directorEntity.Mut<DirectorState>();
            ref var attention = ref directorEntity.Mut<CellAttention>();
            state.Phase = DirectorPhase.Suspicion;
            attention.Noise = 0f;
            attention.Current = 0f;

            phaseSystem.Update();
            Assert.That(scope.ReadDirectorState().Phase, Is.EqualTo(DirectorPhase.Recovery));

            state = directorEntity.Mut<DirectorState>();
            state.PhaseTimer = 19f;
            phaseSystem.Update();
            Assert.That(scope.ReadDirectorState().Phase, Is.EqualTo(DirectorPhase.Recovery));

            phaseSystem.Update();
            Assert.That(scope.ReadDirectorState().Phase, Is.EqualTo(DirectorPhase.Cooldown));

            state = directorEntity.Mut<DirectorState>();
            state.PhaseTimer = 14f;
            phaseSystem.Update();
            Assert.That(scope.ReadDirectorState().Phase, Is.EqualTo(DirectorPhase.Cooldown));

            phaseSystem.Update();
            Assert.That(scope.ReadDirectorState().Phase, Is.EqualTo(DirectorPhase.Ambient));
        }

        [Test]
        public void EncounterSystems_SoloContactAdvancesAndResolvesWithoutSpawnRequest()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(25), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var contactSystem = new EncounterContactDetectionSystem();
            var lifetimeSystem = new EncounterLifetimeSystem();
            var recoverySystem = new EncounterRecoverySystem();
            var phaseSystem = new DirectorPhaseSystem();
            var buildSystem = new SpawnRequestBuildSystem();

            cellTrackingSystem.Update();
            var enemy = scope.CreateEnemy(new Vector3(3f, 0f, 0f), EnemyRole.Swarmer);

            contactSystem.Update();
            lifetimeSystem.Update();
            phaseSystem.Update();
            buildSystem.Update();

            var encounter = scope.ReadEncounterState();
            Assert.That(encounter.Kind, Is.EqualTo(EncounterKind.AmbientSolo));
            Assert.That(encounter.Intensity, Is.EqualTo(EncounterIntensity.Minor));
            Assert.That(encounter.TimeAlive, Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(encounter.AliveEnemyCount, Is.EqualTo(1));
            Assert.That(encounter.EscalationAllowed, Is.False);
            Assert.That(scope.ReadDirectorState().Phase, Is.EqualTo(DirectorPhase.Contact));
            Assert.That(scope.CountSpawnRequestEnemies(), Is.EqualTo(0));

            enemy.Set<IsDiedTag>();
            lifetimeSystem.Update();
            recoverySystem.Update();
            buildSystem.Update();

            Assert.That(scope.HasEncounterState(), Is.False);
            Assert.That(scope.ReadDirectorState().Phase, Is.EqualTo(DirectorPhase.Recovery));
            Assert.That(scope.CountSpawnRequestEnemies(), Is.EqualTo(0));
        }

        [Test]
        public void DirectorSystems_IdenticalInputState_ProducesDeterministicResults()
        {
            var first = RunDeterministicDirectorSequence();
            var second = RunDeterministicDirectorSequence();

            Assert.That(second.Phase, Is.EqualTo(first.Phase));
            Assert.That(second.PhaseTimer, Is.EqualTo(first.PhaseTimer).Within(0.001f));
            Assert.That(second.TimeSinceLastPressureEvent, Is.EqualTo(first.TimeSinceLastPressureEvent).Within(0.001f));
            Assert.That(second.AttentionCurrent, Is.EqualTo(first.AttentionCurrent).Within(0.001f));
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
            directorEntity.Mut<DirectorState>().Phase = DirectorPhase.PressureEvent;

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
        public void SpawnSourceSelectionSystem_PressureSelection_RejectsAmbientOnlySources()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(32), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var selectionSystem = new SpawnSourceSelectionSystem();

            cellTrackingSystem.Update();
            var directorEntity = scope.GetDirectorEntity();
            directorEntity.Mut<DirectorState>().Phase = DirectorPhase.PressureEvent;

            scope.CreateSpawnSource(new Vector3(20f, 0f, 0f), isActive: true, kind: SpawnSourceKind.AmbientPoint, allowsAmbient: true);
            selectionSystem.Update();
            Assert.That(directorEntity.Has<SelectedSpawnSource>(), Is.False);

            scope.CreateSpawnSource(new Vector3(25f, 0f, 0f), isActive: true, kind: SpawnSourceKind.Rift, allowsPressureEvent: true);
            selectionSystem.Update();

            Assert.That(directorEntity.Has<SelectedSpawnSource>(), Is.True);
            Assert.That(directorEntity.Read<SelectedSpawnSource>().SourceKind, Is.EqualTo(SpawnSourceKind.Rift));
        }

        [Test]
        public void SpawnRequestValidationSystem_AmbientOnlySource_CannotValidatePressureRequest()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(34), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var validationSystem = new SpawnRequestValidationSystem();

            cellTrackingSystem.Update();
            var directorEntity = scope.GetDirectorEntity();
            directorEntity.Mut<DirectorState>().Phase = DirectorPhase.PressureEvent;
            directorEntity.Mut<ThreatBudget>().Current = 20f;
            var source = scope.CreateSpawnSource(new Vector3(20f, 0f, 0f), isActive: true, kind: SpawnSourceKind.AmbientPoint, allowsAmbient: true);
            var request = scope.CreateSpawnRequest(source, EnemyRole.Swarmer, count: 1);
            var requestGid = request.GID;

            validationSystem.Update();

            Assert.That(requestGid.TryUnpack<ServerWT>(out _), Is.False);
        }

        [Test]
        public void SpawnSourcePlacementSeedSystem_OpenWorldSpawnPlacements_CreateActiveSources()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            var system = new SpawnSourcePlacementSeedSystem();
            var chunkId = new WorldChunkId(0, 0);

            system.Init();
            try
            {
                SendChunkCompleted(chunkId, new[]
                {
                    new SpawnPlacement(new SpawnPlacementKindId(1), chunkId, new Vector3(20f, 0f, 0f), 0f, 1f),
                    new SpawnPlacement(new SpawnPlacementKindId(2), chunkId, new Vector3(35f, 0f, 0f), 0f, 1.1f)
                });

                system.Update();

                var ambientSources = 0;
                var rifts = 0;
                foreach (var source in SW.Query<All<SpawnSource, SpawnSourcePlacementRef>>().Entities())
                {
                    ref readonly var placement = ref source.Read<SpawnSourcePlacementRef>();
                    ref readonly var spawnSource = ref source.Read<SpawnSource>();

                    Assert.That(placement.ChunkId, Is.EqualTo(chunkId));
                    Assert.That(spawnSource.IsActive, Is.True);
                    Assert.That(spawnSource.Radius, Is.GreaterThan(0f));

                    if (spawnSource.Kind == SpawnSourceKind.AmbientPoint)
                    {
                        Assert.That(spawnSource.Type, Is.EqualTo(SpawnSourceType.Burrow));
                        Assert.That(spawnSource.AllowsAmbient, Is.True);
                        Assert.That(spawnSource.AllowsEscalation, Is.False);
                        Assert.That(spawnSource.AllowsPressureEvent, Is.False);
                        ambientSources++;
                    }
                    else if (spawnSource.Kind == SpawnSourceKind.Rift)
                    {
                        Assert.That(spawnSource.Type, Is.EqualTo(SpawnSourceType.Rift));
                        Assert.That(spawnSource.AllowsAmbient, Is.False);
                        Assert.That(spawnSource.AllowsEscalation, Is.True);
                        Assert.That(spawnSource.AllowsPressureEvent, Is.True);
                        rifts++;
                    }
                }

                Assert.That(ambientSources, Is.EqualTo(1));
                Assert.That(rifts, Is.EqualTo(1));
            }
            finally
            {
                system.Destroy();
            }
        }

        [Test]
        public void SpawnSourcePlacementSeedSystem_UnknownPlacementKind_FailsFast()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            var system = new SpawnSourcePlacementSeedSystem();
            var chunkId = new WorldChunkId(2, 0);

            system.Init();
            try
            {
                SendChunkCompleted(chunkId, new[]
                {
                    new SpawnPlacement(new SpawnPlacementKindId(250), chunkId, new Vector3(20f, 0f, 0f), 0f, 1f)
                });

                Assert.Throws<InvalidOperationException>(() => system.Update());
            }
            finally
            {
                system.Destroy();
            }
        }

        [Test]
        public void SpawnSourcePlacementSeedSystem_RepeatedChunkGeneration_ReplacesChunkSources()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            var system = new SpawnSourcePlacementSeedSystem();
            var chunkId = new WorldChunkId(1, 0);

            system.Init();
            try
            {
                SendChunkCompleted(chunkId, new[]
                {
                    new SpawnPlacement(new SpawnPlacementKindId(1), chunkId, new Vector3(20f, 0f, 0f), 0f, 1f)
                });
                system.Update();

                SendChunkCompleted(chunkId, new[]
                {
                    new SpawnPlacement(new SpawnPlacementKindId(1), chunkId, new Vector3(25f, 0f, 0f), 0f, 1f),
                    new SpawnPlacement(new SpawnPlacementKindId(2), chunkId, new Vector3(35f, 0f, 0f), 0f, 1f)
                });
                system.Update();

                Assert.That(scope.CountSpawnSources(chunkId), Is.EqualTo(2));
            }
            finally
            {
                system.Destroy();
            }
        }

        [Test]
        public void SpawnRequestBuildSystem_PressureEventBudgets_CreateExpectedRoleCounts()
        {
            var low = BuildRequestsForBudget(31f, DirectorPhase.PressureEvent);
            Assert.That(low.Swarmers, Is.EqualTo(10));
            Assert.That(low.Markers, Is.EqualTo(0));
            Assert.That(low.Anchors, Is.EqualTo(0));

            var medium = BuildRequestsForBudget(60f, DirectorPhase.PressureEvent);
            Assert.That(medium.Swarmers, Is.EqualTo(14));
            Assert.That(medium.Markers, Is.EqualTo(1));
            Assert.That(medium.Anchors, Is.EqualTo(0));

            var high = BuildRequestsForBudget(80f, DirectorPhase.PressureEvent);
            Assert.That(high.Swarmers, Is.EqualTo(18));
            Assert.That(high.Markers, Is.EqualTo(1));
            Assert.That(high.Anchors, Is.EqualTo(1));
        }

        [Test]
        public void SpawnRequestBuildSystem_AmbientContactAndRecovery_DoNotCreateWaveRequests()
        {
            Assert.That(BuildRequestsForBudget(80f, DirectorPhase.Ambient).Total, Is.EqualTo(0));
            Assert.That(BuildRequestsForBudget(31f, DirectorPhase.Contact).Total, Is.EqualTo(0));
            Assert.That(BuildRequestsForBudget(60f, DirectorPhase.Contact).Total, Is.EqualTo(0));
            Assert.That(BuildRequestsForBudget(80f, DirectorPhase.Contact).Total, Is.EqualTo(0));
            Assert.That(BuildRequestsForBudget(80f, DirectorPhase.Recovery).Total, Is.EqualTo(0));
        }

        [Test]
        public void AmbientSystems_AmbientEnabledSource_CreatesSoloRequest()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(43), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var scanSystem = new AmbientWorldInterestScanSystem();
            var requestSystem = new AmbientEncounterSpawnRequestSystem();

            cellTrackingSystem.Update();
            scope.CreateSpawnSource(new Vector3(20f, 0f, 0f), isActive: true, kind: SpawnSourceKind.AmbientPoint, allowsAmbient: true, allowsEscalation: false, allowsPressureEvent: false);

            scanSystem.Update();
            requestSystem.Update();

            var request = scope.ReadSingleSpawnRequest();
            Assert.That(request.AmbientKind, Is.EqualTo(AmbientSpawnKind.SoloAnimal));
            Assert.That(request.SourceKind, Is.EqualTo(SpawnSourceKind.AmbientPoint));
            Assert.That(request.Role, Is.EqualTo(EnemyRole.Swarmer));
            Assert.That(request.Count, Is.EqualTo(1));
        }

        [Test]
        public void AmbientSystems_PressureOnlySource_DoesNotCreateAmbientRequest()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(44), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var scanSystem = new AmbientWorldInterestScanSystem();
            var requestSystem = new AmbientEncounterSpawnRequestSystem();

            cellTrackingSystem.Update();
            scope.CreateSpawnSource(new Vector3(20f, 0f, 0f), isActive: true, kind: SpawnSourceKind.Rift, allowsAmbient: false, allowsEscalation: true, allowsPressureEvent: true);

            scanSystem.Update();
            requestSystem.Update();

            Assert.That(scope.CountSpawnRequestEnemies(), Is.EqualTo(0));
        }

        [Test]
        public void AmbientSystems_CooldownPreventsImmediateRepeatedRespawn()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(45), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var scanSystem = new AmbientWorldInterestScanSystem();
            var requestSystem = new AmbientEncounterSpawnRequestSystem();

            cellTrackingSystem.Update();
            var source = scope.CreateSpawnSource(new Vector3(20f, 0f, 0f), isActive: true, kind: SpawnSourceKind.AmbientPoint, allowsAmbient: true, allowsEscalation: false, allowsPressureEvent: false);
            source.Set(new AmbientSpawnMarker
            {
                Kind = AmbientSpawnKind.SmallPack,
                MinCount = 2,
                MaxCount = 4,
                CooldownSeconds = 10f,
                CooldownRemaining = 0f
            });

            scanSystem.Update();
            requestSystem.Update();
            Assert.That(scope.CountSpawnRequestEnemies(), Is.EqualTo(4));
            Assert.That(source.Read<AmbientSpawnMarker>().CooldownRemaining, Is.GreaterThan(0f));

            scope.DestroySpawnRequests();
            scanSystem.Update();
            requestSystem.Update();

            Assert.That(scope.CountSpawnRequestEnemies(), Is.EqualTo(0));
        }

        [Test]
        public void AmbientSystems_AmbientCapPreventsOverSpawnRequests()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(46), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var scanSystem = new AmbientWorldInterestScanSystem();
            var requestSystem = new AmbientEncounterSpawnRequestSystem();

            cellTrackingSystem.Update();
            for (var i = 0; i < 4; i++)
                scope.CreateEnemy(new Vector3(i, 0f, 0f), EnemyRole.Swarmer);

            scope.CreateSpawnSource(new Vector3(20f, 0f, 0f), isActive: true, kind: SpawnSourceKind.AmbientPoint, allowsAmbient: true, allowsEscalation: false, allowsPressureEvent: false);

            scanSystem.Update();
            requestSystem.Update();

            Assert.That(scope.CountSpawnRequestEnemies(), Is.EqualTo(0));
            Assert.That(scope.ReadCellAliveEnemyCaps().AmbientAliveEnemies, Is.EqualTo(4));
            Assert.That(scope.ReadCellAliveEnemyCaps().AmbientMaxAliveEnemies, Is.EqualTo(4));
        }

        [Test]
        public void AmbientSystems_SoloAmbientKill_RecoversWithoutWave()
        {
            using var scope = new CombatDirectorTestServerWorldScope();
            scope.CreatePlayer(new NetworkPeerId(47), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var scanSystem = new AmbientWorldInterestScanSystem();
            var requestSystem = new AmbientEncounterSpawnRequestSystem();
            var sourceSelectionSystem = new SpawnSourceSelectionSystem();
            var validationSystem = new SpawnRequestValidationSystem();
            var applySystem = new EnemySpawnApplySystem();
            var lifetimeSystem = new EncounterLifetimeSystem();
            var recoverySystem = new EncounterRecoverySystem();
            var phaseSystem = new DirectorPhaseSystem();
            var pressureBuildSystem = new SpawnRequestBuildSystem();

            cellTrackingSystem.Update();
            var directorEntity = scope.GetDirectorEntity();
            var startingBudget = scope.ReadThreatBudget().Current;
            scope.CreateSpawnSource(new Vector3(20f, 0f, 0f), isActive: true, kind: SpawnSourceKind.AmbientPoint, allowsAmbient: true, allowsEscalation: false, allowsPressureEvent: false);

            scanSystem.Update();
            requestSystem.Update();
            sourceSelectionSystem.Update();
            validationSystem.Update();
            applySystem.Update();
            phaseSystem.Update();

            var encounter = scope.ReadEncounterState();
            Assert.That(encounter.Kind, Is.EqualTo(EncounterKind.AmbientSolo));
            Assert.That(encounter.EscalationAllowed, Is.False);
            Assert.That(scope.ReadDirectorState().Phase, Is.EqualTo(DirectorPhase.Contact));
            Assert.That(scope.ReadThreatBudget().Current, Is.EqualTo(startingBudget).Within(0.001f));

            scope.MarkAllEnemiesDied();
            lifetimeSystem.Update();
            recoverySystem.Update();
            pressureBuildSystem.Update();

            Assert.That(scope.HasEncounterState(), Is.False);
            Assert.That(scope.ReadDirectorState().Phase, Is.EqualTo(DirectorPhase.Recovery));
            Assert.That(scope.CountSpawnRequestEnemies(), Is.EqualTo(0));
            Assert.That(directorEntity.Has<SelectedSpawnSource>(), Is.False);
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
            directorEntity.Mut<DirectorState>().Phase = DirectorPhase.PressureEvent;
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
            directorEntity.Mut<DirectorState>().Phase = DirectorPhase.PressureEvent;
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
            var player = scope.CreatePlayer(new NetworkPeerId(37), Vector3.zero);
            var cellTrackingSystem = new CombatCellTrackingSystem();
            var validationSystem = new SpawnRequestValidationSystem();
            var applySystem = new EnemySpawnApplySystem();

            cellTrackingSystem.Update();
            var directorEntity = scope.GetDirectorEntity();
            directorEntity.Mut<DirectorState>().Phase = DirectorPhase.PressureEvent;
            directorEntity.Mut<ThreatBudget>().Current = 20f;
            var source = scope.CreateSpawnSource(new Vector3(60f, 0f, 0f), isActive: true);
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
                    Assert.That(evt.Value.SourceType, Is.EqualTo(SpawnSourceType.Rift));
                    spawnedEvents++;
                }

                Assert.That(spawnedEvents, Is.EqualTo(2));
                Assert.That(scope.CountEnemies(EnemyRole.Swarmer), Is.EqualTo(2));
                Assert.That(scope.CountEnemiesTargeting(player.GID), Is.EqualTo(2));
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
            var attentionInputSystem = new CellAttentionInputSystem();
            var decaySystem = new CellAttentionDecaySystem();
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
                    attentionInputSystem.Update();
                    decaySystem.Update();
                    phaseSystem.Update();
                }

                var state = scope.ReadDirectorState();
                var attention = scope.ReadCellAttention();
                var budget = scope.ReadThreatBudget();
                return new DeterministicDirectorResult(
                    state.Phase,
                    state.PhaseTimer,
                    state.TimeSinceLastPressureEvent,
                    attention.Current,
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
                    typeof(Health).Assembly,
                    typeof(ReplicatedComponentRegistration).Assembly,
                    typeof(ServerWT).Assembly,
                    typeof(PlayerTag).Assembly,
                    typeof(ServerCombatAttackState).Assembly,
                    typeof(AiBotsGameplayFeature).Assembly,
                    typeof(OpenWorldChunkGenerationCompleted).Assembly,
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

            public SW.Entity CreateSpawnSource(
                Vector3 position,
                bool isActive,
                SpawnSourceKind kind = SpawnSourceKind.Rift,
                bool allowsAmbient = false,
                bool allowsEscalation = true,
                bool allowsPressureEvent = true)
            {
                var source = SW.NewEntity<Default>();
                source.Set(new SpawnSource
                {
                    Type = kind == SpawnSourceKind.Rift ? SpawnSourceType.Rift : SpawnSourceType.Burrow,
                    Kind = kind,
                    Position = new Unity.Mathematics.float3(position.x, position.y, position.z),
                    Radius = 5f,
                    IsActive = isActive,
                    AllowsAmbient = allowsAmbient,
                    AllowsEscalation = allowsEscalation,
                    AllowsPressureEvent = allowsPressureEvent
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
                    SourceKind = spawnSource.Kind,
                    Role = role,
                    Count = count,
                    SpawnPosition = spawnSource.Position
                });
                return request;
            }

            public SpawnRequest ReadSingleSpawnRequest()
            {
                var found = false;
                SpawnRequest spawnRequest = default;
                foreach (var request in SW.Query<All<SpawnRequest>>().Entities())
                {
                    if (found)
                        throw new InvalidOperationException("Expected exactly one spawn request in test scope.");

                    spawnRequest = request.Read<SpawnRequest>();
                    found = true;
                }

                if (!found)
                    throw new InvalidOperationException("Spawn request was not created.");

                return spawnRequest;
            }

            public int CountSpawnRequestEnemies()
            {
                var count = 0;
                foreach (var request in SW.Query<All<SpawnRequest>>().Entities())
                    count += request.Read<SpawnRequest>().Count;

                return count;
            }

            public void DestroySpawnRequests()
            {
                var requests = new System.Collections.Generic.List<EntityGID>();
                foreach (var request in SW.Query<All<SpawnRequest>>().Entities())
                    requests.Add(request.GID);

                for (var i = 0; i < requests.Count; i++)
                {
                    if (requests[i].TryUnpack<ServerWT>(out var request))
                        request.Destroy();
                }
            }

            public void MarkAllEnemiesDied()
            {
                foreach (var enemy in SW.Query<All<EnemyTag>>().Entities())
                    enemy.Set<IsDiedTag>();
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

            public int CountEnemiesTargeting(EntityGID target)
            {
                var count = 0;
                foreach (var enemy in SW.Query<All<EnemyTag, SW.Multi<AiBlackboardEntry>>>().Entities())
                {
                    if (AiBlackboardAccess.TryGetEntity(enemy, AiCoreVariableIds.Enemy, out var enemyTarget)
                        && enemyTarget == target)
                    {
                        count++;
                    }
                }

                return count;
            }

            public int CountSpawnSources(WorldChunkId chunkId)
            {
                var count = 0;
                foreach (var source in SW.Query<All<SpawnSourcePlacementRef>>().Entities())
                {
                    if (source.Read<SpawnSourcePlacementRef>().ChunkId == chunkId)
                        count++;
                }

                return count;
            }

            public SW.Entity GetDirectorEntity()
            {
                var found = false;
                SW.Entity directorEntity = default;
                foreach (var entity in SW.Query<All<CombatCell, CellAttention, ThreatBudget, DirectorState>>().Entities())
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

            public CellAttention ReadCellAttention()
            {
                return GetDirectorEntity().Read<CellAttention>();
            }

            public CellAliveEnemyCaps ReadCellAliveEnemyCaps()
            {
                return GetDirectorEntity().Read<CellAliveEnemyCaps>();
            }

            public DirectorState ReadDirectorState()
            {
                return GetDirectorEntity().Read<DirectorState>();
            }

            public bool HasEncounterState()
            {
                return GetDirectorEntity().Has<EncounterState>();
            }

            public EncounterState ReadEncounterState()
            {
                return GetDirectorEntity().Read<EncounterState>();
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
            public readonly float TimeSinceLastPressureEvent;
            public readonly float AttentionCurrent;
            public readonly float BudgetCurrent;
            public readonly float BudgetAccumulationPerSecond;

            public DeterministicDirectorResult(
                DirectorPhase phase,
                float phaseTimer,
                float timeSinceLastPressureEvent,
                float attentionCurrent,
                float budgetCurrent,
                float budgetAccumulationPerSecond)
            {
                Phase = phase;
                PhaseTimer = phaseTimer;
                TimeSinceLastPressureEvent = timeSinceLastPressureEvent;
                AttentionCurrent = attentionCurrent;
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

        private static void SendChunkCompleted(WorldChunkId chunkId, SpawnPlacement[] spawnPlacements)
        {
            SW.SendEvent(new OpenWorldChunkGenerationCompleted(
                chunkId,
                128f,
                lod: 0,
                outputs: GenerationOutputMask.Placements,
                terrainMesh: null,
                physicsMesh: null,
                navMeshSourceMesh: null,
                resourcePlacements: Array.Empty<ResourcePlacement>(),
                spawnPlacements: spawnPlacements));
        }

        private struct SpawnRequestRoleCounts
        {
            public int Swarmers;
            public int Markers;
            public int Anchors;

            public int Total => Swarmers + Markers + Anchors;
        }
    }
}
