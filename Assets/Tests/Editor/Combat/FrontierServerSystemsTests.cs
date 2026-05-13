using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Build;
using StaticMlp.Features.Combat;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class FrontierServerSystemsTests
    {
        [Test]
        public void CreateSettlementAnchor_AddsDefaultFrontierStateComponents()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(SettlementAnchorCatalog.HomeCampId, Vector3.zero, Stage1SettlementProgressStage.BuildPrepared);

            Assert.That(anchor.Has<ExpeditionAvailabilityState>(), Is.True);
            Assert.That(anchor.Has<ActiveExpeditionState>(), Is.True);
            Assert.That(anchor.Has<ThreatState>(), Is.True);
            Assert.That(anchor.Has<RaidScheduleState>(), Is.True);
            Assert.That(anchor.Has<BossBuildPreparationState>(), Is.True);
            Assert.That(anchor.Has<BossPreparedBuildSnapshot>(), Is.True);
            Assert.That(anchor.Has<BossEncounterState>(), Is.True);
            Assert.That(anchor.Read<ThreatState>().Phase, Is.EqualTo(ThreatPhase.Calm));
            Assert.That(anchor.Read<RaidScheduleState>().Status, Is.EqualTo(RaidScheduleStatus.None));
            Assert.That(anchor.Read<BossBuildPreparationState>().Status, Is.EqualTo(BossBuildPreparationStatus.None));
            Assert.That(anchor.Read<BossEncounterState>().Status, Is.EqualTo(BossEncounterStatus.Unavailable));
        }

        [Test]
        public void ExpeditionAvailabilitySystem_UnlocksOnlyAfterBuildPrepared()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(SettlementAnchorCatalog.HomeCampId, Vector3.zero, Stage1SettlementProgressStage.CampRepaired);

            new ServerFrontierExpeditionAvailabilitySystem().Update();

            Assert.That(anchor.Read<ExpeditionAvailabilityState>().Status, Is.EqualTo(ExpeditionAvailabilityStatus.Unavailable));

            anchor.Set(new Stage1SettlementProgression
            {
                AnchorId = SettlementAnchorCatalog.HomeCampId.Value,
                Stage = Stage1SettlementProgressStage.BuildPrepared
            });

            new ServerFrontierExpeditionAvailabilitySystem().Update();

            Assert.That(anchor.Read<ExpeditionAvailabilityState>().Status, Is.EqualTo(ExpeditionAvailabilityStatus.Available));
            Assert.That(anchor.Read<ExpeditionAvailabilityState>().ExpeditionIdValue, Is.EqualTo(ExpeditionCatalog.NearbyRaiderCampId.Value));
        }

        [Test]
        public void StartExpeditionSystem_StartsEncounterOnlyWhenNoRaidIsPending()
        {
            using var scope = new CombatTestServerWorldScope();
            var owner = new NetworkPeerId(1);
            scope.CreatePlayer(owner, Vector3.zero);
            var anchor = scope.CreateSettlementAnchor(SettlementAnchorCatalog.HomeCampId, Vector3.zero, Stage1SettlementProgressStage.BuildPrepared);
            new ServerFrontierExpeditionAvailabilitySystem().Update();

            var startSystem = new ServerFrontierStartExpeditionSystem();
            var spawnSystem = new ServerFrontierEncounterBotSpawnSystem();
            startSystem.Init();
            spawnSystem.Init();

            var request = new StartExpeditionRequestEvent(SettlementAnchorCatalog.HomeCampId, ExpeditionCatalog.NearbyRaiderCampId);
            SW.SendEvent(new NetworkEventFromClient<StartExpeditionRequestEvent>(owner, in request));
            startSystem.Update();
            spawnSystem.Update();

            Assert.That(anchor.Read<ActiveExpeditionState>().Status, Is.EqualTo(ExpeditionActivityStatus.Active));
            Assert.That(anchor.Read<ExpeditionAvailabilityState>().Status, Is.EqualTo(ExpeditionAvailabilityStatus.Unavailable));
            var existingParticipants = CountParticipants(FrontierEncounterKind.Expedition, ExpeditionCatalog.NearbyRaiderCampId.Value);
            Assert.That(existingParticipants, Is.GreaterThan(0));

            var blockedAnchor = scope.CreateSettlementAnchor(new SettlementAnchorId(2), new Vector3(10f, 0f, 0f), Stage1SettlementProgressStage.BuildPrepared);
            blockedAnchor.Set(new ExpeditionAvailabilityState
            {
                ExpeditionIdValue = ExpeditionCatalog.NearbyRaiderCampId.Value,
                Status = ExpeditionAvailabilityStatus.Available
            });
            blockedAnchor.Set(new ActiveExpeditionState
            {
                Status = ExpeditionActivityStatus.None
            });
            blockedAnchor.Set(new ThreatState
            {
                Phase = ThreatPhase.RaidPending
            });
            blockedAnchor.Set(new RaidScheduleState
            {
                RaidIdValue = RaidCatalog.RaiderCounterattackId.Value,
                Status = RaidScheduleStatus.Pending,
                ActivateAtTick = 10
            });

            var blockedRequest = new StartExpeditionRequestEvent(new SettlementAnchorId(2), ExpeditionCatalog.NearbyRaiderCampId);
            SW.SendEvent(new NetworkEventFromClient<StartExpeditionRequestEvent>(owner, in blockedRequest));
            startSystem.Update();
            spawnSystem.Update();

            Assert.That(blockedAnchor.Read<ActiveExpeditionState>().Status, Is.EqualTo(ExpeditionActivityStatus.None));
            Assert.That(
                CountParticipants(FrontierEncounterKind.Expedition, ExpeditionCatalog.NearbyRaiderCampId.Value),
                Is.EqualTo(existingParticipants));

            spawnSystem.Destroy();
            startSystem.Destroy();
        }

        [Test]
        public void ExpeditionResolution_ClearsExpeditionAndSchedulesRaidByServerTick()
        {
            using var scope = new CombatTestServerWorldScope();
            var owner = new NetworkPeerId(1);
            scope.CreatePlayer(owner, Vector3.zero);
            var anchor = scope.CreateSettlementAnchor(SettlementAnchorCatalog.HomeCampId, Vector3.zero, Stage1SettlementProgressStage.BuildPrepared);
            scope.CreateSettlementSharedResources();
            new ServerFrontierExpeditionAvailabilitySystem().Update();

            StartExpedition(scope, owner);
            MarkParticipantsAsDead(FrontierEncounterKind.Expedition, ExpeditionCatalog.NearbyRaiderCampId.Value);
            new ServerBotAiDeathSystem().Update();
            var applyRewardSystem = new ServerStage1RewardApplicationSystem();
            var escalateThreatSystem = new ServerFrontierProgressionFlagThreatEscalationSystem();
            applyRewardSystem.Init();
            escalateThreatSystem.Init();

            new ServerFrontierExpeditionResolutionSystem().Update();
            applyRewardSystem.Update();
            escalateThreatSystem.Update();

            Assert.That(anchor.Read<ActiveExpeditionState>().Status, Is.EqualTo(ExpeditionActivityStatus.Cleared));
            Assert.That(anchor.Read<Stage1ProgressionState>().HasAppliedReward(RewardPackageCatalog.RecoveredWarCacheId), Is.True);
            Assert.That(anchor.Read<Stage1ProgressionState>().HasFlag(ProgressFlagCatalog.RecoveredWarCacheAppliedId), Is.True);
            Assert.That(anchor.Read<Stage1ProgressionState>().BossPreparationTokens, Is.EqualTo(1));
            Assert.That(anchor.Read<ThreatState>().Phase, Is.EqualTo(ThreatPhase.RaidPending));
            Assert.That(anchor.Read<RaidScheduleState>().Status, Is.EqualTo(RaidScheduleStatus.Pending));
            Assert.That(anchor.Read<RaidScheduleState>().ActivateAtTick, Is.EqualTo(scope.SimulationTime.DeadlineAfter(5f)));

            var sharedResources = SettlementSharedResourcesQuery.GetServerEntity().Read<SettlementSharedResources>();
            Assert.That(sharedResources.Wood, Is.EqualTo(70));
            Assert.That(sharedResources.Stone, Is.EqualTo(35));

            escalateThreatSystem.Destroy();
            applyRewardSystem.Destroy();
        }

        [Test]
        public void RaidActivationSystem_SpawnsRaidWhenTickDeadlineIsReached()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(SettlementAnchorCatalog.HomeCampId, Vector3.zero, Stage1SettlementProgressStage.BuildPrepared);

            anchor.Set(new ThreatState
            {
                Phase = ThreatPhase.RaidPending,
                ThreatValue = 2
            });
            anchor.Set(new RaidScheduleState
            {
                RaidIdValue = RaidCatalog.RaiderCounterattackId.Value,
                Status = RaidScheduleStatus.Pending,
                ActivateAtTick = 3
            });

            var spawnSystem = new ServerFrontierEncounterBotSpawnSystem();
            spawnSystem.Init();

            scope.SetSimulationTime(serverTick: 2);
            new ServerFrontierRaidActivationSystem().Update();
            spawnSystem.Update();
            Assert.That(CountParticipants(FrontierEncounterKind.Raid, RaidCatalog.RaiderCounterattackId.Value), Is.EqualTo(0));

            scope.SetSimulationTime(serverTick: 3);
            new ServerFrontierRaidActivationSystem().Update();
            spawnSystem.Update();

            Assert.That(anchor.Read<RaidScheduleState>().Status, Is.EqualTo(RaidScheduleStatus.Active));
            Assert.That(anchor.Read<ThreatState>().Phase, Is.EqualTo(ThreatPhase.RaidActive));
            Assert.That(CountParticipants(FrontierEncounterKind.Raid, RaidCatalog.RaiderCounterattackId.Value), Is.GreaterThan(0));

            spawnSystem.Destroy();
        }

        [Test]
        public void RaidResolution_ClearsThreatAndScheduleAfterRaidDeaths()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(SettlementAnchorCatalog.HomeCampId, Vector3.zero, Stage1SettlementProgressStage.BuildPrepared);

            anchor.Set(new ThreatState
            {
                Phase = ThreatPhase.RaidActive,
                ThreatValue = 2
            });
            anchor.Set(new RaidScheduleState
            {
                RaidIdValue = RaidCatalog.RaiderCounterattackId.Value,
                Status = RaidScheduleStatus.Active,
                ActivateAtTick = 3
            });

            var participant = scope.CreateMonsterWithHealth(new Vector3(2f, 0f, 2f));
            participant.Set(new FrontierEncounterParticipant
            {
                AnchorId = SettlementAnchorCatalog.HomeCampId.Value,
                EncounterKind = FrontierEncounterKind.Raid,
                SourceId = RaidCatalog.RaiderCounterattackId.Value
            });
            participant.Set<AiAgentTag>();
            participant.Set<IsDiedTag>();

            var applyRaidDefenseProgressionSystem = new ServerStage1RaidDefenseProgressionSystem();
            applyRaidDefenseProgressionSystem.Init();
            new ServerBotAiDeathSystem().Update();
            new ServerFrontierRaidResolutionSystem().Update();
            applyRaidDefenseProgressionSystem.Update();

            Assert.That(anchor.Read<RaidScheduleState>().Status, Is.EqualTo(RaidScheduleStatus.None));
            Assert.That(anchor.Read<RaidScheduleState>().ActivateAtTick, Is.EqualTo(0));
            Assert.That(anchor.Read<ThreatState>().Phase, Is.EqualTo(ThreatPhase.Calm));
            Assert.That(anchor.Read<ThreatState>().ThreatValue, Is.EqualTo(0));
            Assert.That(anchor.Read<Stage1ProgressionState>().HasFlag(ProgressFlagCatalog.CounterattackDefendedId), Is.True);

            applyRaidDefenseProgressionSystem.Destroy();
        }

        [Test]
        public void BossPreparationSystem_RejectsRequestWithoutDefenseFlag()
        {
            using var scope = new CombatTestServerWorldScope();
            var owner = new NetworkPeerId(1);
            var player = scope.CreatePlayer(owner, Vector3.zero);
            var anchor = scope.CreateSettlementAnchor(SettlementAnchorCatalog.HomeCampId, Vector3.zero, Stage1SettlementProgressStage.BuildPrepared);
            InitializeStage1BossPath(scope);

            ref var progression = ref anchor.Mut<Stage1ProgressionState>();
            progression.GrantBossPreparationTokens(1);

            player.Set(new OwnerBuildSelection
            {
                PrimaryModuleId = BuildModuleCatalog.FireFlaskModuleId
            });
            player.Set(Stage1BuildRules.CreatePreparedSnapshot(player.Read<OwnerBuildSelection>()));

            var bossPreparationSystem = new ServerStage1BossPreparationProgressionSystem();
            bossPreparationSystem.Init();
            var request = new PrepareBossRequestEvent(SettlementAnchorCatalog.HomeCampId);
            SW.SendEvent(new NetworkEventFromClient<PrepareBossRequestEvent>(owner, in request));
            bossPreparationSystem.Update();

            Assert.That(anchor.Read<Stage1ProgressionState>().BossPreparationTokens, Is.EqualTo(1));
            Assert.That(anchor.Read<Stage1ProgressionState>().HasFlag(ProgressFlagCatalog.BossUnlockedId), Is.False);
            Assert.That(anchor.Read<BossBuildPreparationState>().Status, Is.EqualTo(BossBuildPreparationStatus.None));
            Assert.That(anchor.Read<BossPreparedBuildSnapshot>().PrimaryModuleIdValue, Is.EqualTo(0));

            bossPreparationSystem.Destroy();
        }

        [Test]
        public void BossPreparationSystem_CommitsCurrentBuildAndUnlocksBoss()
        {
            using var scope = new CombatTestServerWorldScope();
            var owner = new NetworkPeerId(1);
            var player = scope.CreatePlayer(owner, Vector3.zero);
            var anchor = scope.CreateSettlementAnchor(SettlementAnchorCatalog.HomeCampId, Vector3.zero, Stage1SettlementProgressStage.BuildPrepared);
            InitializeStage1BossPath(scope);

            ref var progression = ref anchor.Mut<Stage1ProgressionState>();
            progression.ApplyFlag(ProgressFlagCatalog.CounterattackDefendedId);
            progression.GrantBossPreparationTokens(1);

            player.Set(new OwnerBuildSelection
            {
                PrimaryModuleId = BuildModuleCatalog.FireFlaskModuleId
            });
            player.Set(Stage1BuildRules.CreatePreparedSnapshot(player.Read<OwnerBuildSelection>()));

            var bossPreparationSystem = new ServerStage1BossPreparationProgressionSystem();
            bossPreparationSystem.Init();
            var request = new PrepareBossRequestEvent(SettlementAnchorCatalog.HomeCampId);
            SW.SendEvent(new NetworkEventFromClient<PrepareBossRequestEvent>(owner, in request));
            bossPreparationSystem.Update();

            ref readonly var committedState = ref anchor.Read<BossBuildPreparationState>();
            ref readonly var committedSnapshot = ref anchor.Read<BossPreparedBuildSnapshot>();
            ref readonly var playerSnapshot = ref player.Read<PreparedBuildSnapshot>();

            Assert.That(anchor.Read<Stage1ProgressionState>().BossPreparationTokens, Is.EqualTo(0));
            Assert.That(anchor.Read<Stage1ProgressionState>().HasFlag(ProgressFlagCatalog.BossUnlockedId), Is.True);
            Assert.That(committedState.Status, Is.EqualTo(BossBuildPreparationStatus.Committed));
            Assert.That(committedSnapshot.ArchetypeIdValue, Is.EqualTo(playerSnapshot.ArchetypeId.Value));
            Assert.That(committedSnapshot.PrimaryModuleIdValue, Is.EqualTo(playerSnapshot.PrimaryModuleId.Value));
            Assert.That(committedSnapshot.PreparedAbilityId, Is.EqualTo(playerSnapshot.PreparedAbilityId));
            Assert.That(committedSnapshot.FallbackAbilityId, Is.EqualTo(playerSnapshot.FallbackAbilityId));

            var buildSelectionSystem = new ServerReceivePrepareBuildCommandSystem();
            buildSelectionSystem.Init();
            var buildRequest = new PrepareBuildCommand(
                SettlementAnchorCatalog.HomeCampId,
                BuildModuleCatalog.PoisonArrowModuleId);
            SW.SendEvent(new NetworkEventFromClient<PrepareBuildCommand>(owner, in buildRequest));
            buildSelectionSystem.Update();

            Assert.That(player.Read<OwnerBuildSelection>().PrimaryModuleId, Is.EqualTo(BuildModuleCatalog.FireFlaskModuleId));

            buildSelectionSystem.Destroy();
            bossPreparationSystem.Destroy();
        }

        [Test]
        public void BossAvailabilitySystem_UnlocksOnlyThroughBossEncounterState()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(SettlementAnchorCatalog.HomeCampId, Vector3.zero, Stage1SettlementProgressStage.BuildPrepared);
            InitializeStage1BossPath(scope);

            new ServerFrontierExpeditionAvailabilitySystem().Update();
            new ServerFrontierBossAvailabilitySystem().Update();

            Assert.That(anchor.Read<ExpeditionAvailabilityState>().ExpeditionIdValue, Is.EqualTo(ExpeditionCatalog.NearbyRaiderCampId.Value));
            Assert.That(anchor.Read<BossEncounterState>().Status, Is.EqualTo(BossEncounterStatus.Unavailable));

            ref var progression = ref anchor.Mut<Stage1ProgressionState>();
            progression.ApplyFlag(ProgressFlagCatalog.BossUnlockedId);

            new ServerFrontierBossAvailabilitySystem().Update();

            Assert.That(anchor.Read<ExpeditionAvailabilityState>().ExpeditionIdValue, Is.EqualTo(ExpeditionCatalog.NearbyRaiderCampId.Value));
            Assert.That(anchor.Read<BossEncounterState>().BossIdValue, Is.EqualTo(BossCatalog.RaiderChiefId.Value));
            Assert.That(anchor.Read<BossEncounterState>().Status, Is.EqualTo(BossEncounterStatus.Available));
        }

        [Test]
        public void BossEncounterFlow_StartsAndCompletesVerticalSlice()
        {
            using var scope = new CombatTestServerWorldScope();
            var owner = new NetworkPeerId(1);
            var player = scope.CreatePlayer(owner, Vector3.zero);
            var anchor = scope.CreateSettlementAnchor(SettlementAnchorCatalog.HomeCampId, Vector3.zero, Stage1SettlementProgressStage.BuildPrepared);
            InitializeStage1BossPath(scope);

            ref var progression = ref anchor.Mut<Stage1ProgressionState>();
            progression.ApplyFlag(ProgressFlagCatalog.CounterattackDefendedId);
            progression.GrantBossPreparationTokens(1);

            player.Set(new OwnerBuildSelection
            {
                PrimaryModuleId = BuildModuleCatalog.FireFlaskModuleId
            });
            player.Set(Stage1BuildRules.CreatePreparedSnapshot(player.Read<OwnerBuildSelection>()));

            var bossPreparationSystem = new ServerStage1BossPreparationProgressionSystem();
            bossPreparationSystem.Init();
            var prepareRequest = new PrepareBossRequestEvent(SettlementAnchorCatalog.HomeCampId);
            SW.SendEvent(new NetworkEventFromClient<PrepareBossRequestEvent>(owner, in prepareRequest));
            bossPreparationSystem.Update();

            new ServerFrontierBossAvailabilitySystem().Update();
            Assert.That(anchor.Read<BossEncounterState>().Status, Is.EqualTo(BossEncounterStatus.Available));

            var completionReceiver = SW.RegisterEventReceiver<VerticalSliceCompleteEvent>();
            var bossStartSystem = new ServerFrontierStartBossEncounterSystem();
            var spawnSystem = new ServerFrontierEncounterBotSpawnSystem();
            bossStartSystem.Init();
            spawnSystem.Init();

            var startBossRequest = new StartBossEncounterRequestEvent(SettlementAnchorCatalog.HomeCampId, BossCatalog.RaiderChiefId);
            SW.SendEvent(new NetworkEventFromClient<StartBossEncounterRequestEvent>(owner, in startBossRequest));
            bossStartSystem.Update();
            spawnSystem.Update();

            Assert.That(anchor.Read<BossEncounterState>().Status, Is.EqualTo(BossEncounterStatus.Active));
            var bossParticipants = CountParticipants(FrontierEncounterKind.Boss, BossCatalog.RaiderChiefId.Value);
            Assert.That(bossParticipants, Is.GreaterThan(0));

            SW.SendEvent(new NetworkEventFromClient<StartBossEncounterRequestEvent>(owner, in startBossRequest));
            bossStartSystem.Update();
            spawnSystem.Update();

            Assert.That(CountParticipants(FrontierEncounterKind.Boss, BossCatalog.RaiderChiefId.Value), Is.EqualTo(bossParticipants));

            MarkParticipantsAsDead(FrontierEncounterKind.Boss, BossCatalog.RaiderChiefId.Value);
            new ServerBotAiDeathSystem().Update();
            new ServerFrontierBossResolutionSystem().Update();

            Assert.That(anchor.Read<BossEncounterState>().Status, Is.EqualTo(BossEncounterStatus.Defeated));

            var completions = 0;
            foreach (var evt in completionReceiver)
            {
                completions++;
                Assert.That(evt.Value.AnchorId, Is.EqualTo(SettlementAnchorCatalog.HomeCampId));
            }

            Assert.That(completions, Is.EqualTo(1));

            SW.DeleteEventReceiver(ref completionReceiver);
            spawnSystem.Destroy();
            bossStartSystem.Destroy();
            bossPreparationSystem.Destroy();
        }

        private static void StartExpedition(CombatTestServerWorldScope scope, NetworkPeerId owner)
        {
            var startSystem = new ServerFrontierStartExpeditionSystem();
            var spawnSystem = new ServerFrontierEncounterBotSpawnSystem();
            startSystem.Init();
            spawnSystem.Init();

            var request = new StartExpeditionRequestEvent(SettlementAnchorCatalog.HomeCampId, ExpeditionCatalog.NearbyRaiderCampId);
            SW.SendEvent(new NetworkEventFromClient<StartExpeditionRequestEvent>(owner, in request));
            startSystem.Update();
            spawnSystem.Update();

            spawnSystem.Destroy();
            startSystem.Destroy();
        }

        private static void InitializeStage1BossPath(CombatTestServerWorldScope scope)
        {
            scope.CreateSettlementSharedResources();
        }

        private static void MarkParticipantsAsDead(FrontierEncounterKind encounterKind, ushort sourceId)
        {
            foreach (var entity in SW.Query<All<FrontierEncounterParticipant>>().Entities())
            {
                ref readonly var participant = ref entity.Read<FrontierEncounterParticipant>();
                if (participant.EncounterKind != encounterKind || participant.SourceId != sourceId)
                    continue;

                entity.Set<IsDiedTag>();
            }
        }

        private static int CountParticipants(FrontierEncounterKind encounterKind, ushort sourceId)
        {
            var count = 0;
            foreach (var entity in SW.Query<All<FrontierEncounterParticipant>>().Entities())
            {
                ref readonly var participant = ref entity.Read<FrontierEncounterParticipant>();
                if (participant.EncounterKind != encounterKind || participant.SourceId != sourceId)
                    continue;

                count++;
            }

            return count;
        }
    }
}
