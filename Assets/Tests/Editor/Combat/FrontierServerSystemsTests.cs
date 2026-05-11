using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Combat;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class FrontierServerSystemsTests
    {
        [Test]
        public void AnchorInitSystem_AddsDefaultFrontierStateComponents()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(SettlementAnchorCatalog.HomeCampId, Vector3.zero, Stage1SettlementProgressStage.BuildPrepared);

            new ServerFrontierAnchorInitSystem().Update();

            Assert.That(anchor.Has<ExpeditionAvailabilityState>(), Is.True);
            Assert.That(anchor.Has<ActiveExpeditionState>(), Is.True);
            Assert.That(anchor.Has<ThreatState>(), Is.True);
            Assert.That(anchor.Has<RaidScheduleState>(), Is.True);
            Assert.That(anchor.Read<ThreatState>().Phase, Is.EqualTo(ThreatPhase.Calm));
            Assert.That(anchor.Read<RaidScheduleState>().Status, Is.EqualTo(RaidScheduleStatus.None));
        }

        [Test]
        public void ExpeditionAvailabilitySystem_UnlocksOnlyAfterBuildPrepared()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(SettlementAnchorCatalog.HomeCampId, Vector3.zero, Stage1SettlementProgressStage.CampRepaired);
            new ServerFrontierAnchorInitSystem().Update();

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
            new ServerFrontierAnchorInitSystem().Update();
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
            new ServerFrontierAnchorInitSystem().Update();
            new ServerFrontierExpeditionAvailabilitySystem().Update();

            StartExpedition(scope, owner);
            MarkParticipantsAsDead(FrontierEncounterKind.Expedition, ExpeditionCatalog.NearbyRaiderCampId.Value);
            new ServerBotAiDeathSystem().Update();
            new ServerFrontierExpeditionResolutionSystem().Update();

            Assert.That(anchor.Read<ActiveExpeditionState>().Status, Is.EqualTo(ExpeditionActivityStatus.Cleared));
            Assert.That(anchor.Read<ThreatState>().Phase, Is.EqualTo(ThreatPhase.RaidPending));
            Assert.That(anchor.Read<RaidScheduleState>().Status, Is.EqualTo(RaidScheduleStatus.Pending));
            Assert.That(anchor.Read<RaidScheduleState>().ActivateAtTick, Is.EqualTo(scope.SimulationTime.DeadlineAfter(5f)));
        }

        [Test]
        public void RaidActivationSystem_SpawnsRaidWhenTickDeadlineIsReached()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(SettlementAnchorCatalog.HomeCampId, Vector3.zero, Stage1SettlementProgressStage.BuildPrepared);
            new ServerFrontierAnchorInitSystem().Update();

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
            new ServerFrontierAnchorInitSystem().Update();

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

            new ServerBotAiDeathSystem().Update();
            new ServerFrontierRaidResolutionSystem().Update();

            Assert.That(anchor.Read<RaidScheduleState>().Status, Is.EqualTo(RaidScheduleStatus.None));
            Assert.That(anchor.Read<RaidScheduleState>().ActivateAtTick, Is.EqualTo(0));
            Assert.That(anchor.Read<ThreatState>().Phase, Is.EqualTo(ThreatPhase.Calm));
            Assert.That(anchor.Read<ThreatState>().ThreatValue, Is.EqualTo(0));
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
