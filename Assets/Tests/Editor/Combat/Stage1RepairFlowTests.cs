using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.Build;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Features.Stage1;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class Stage1RepairFlowTests
    {
        [Test]
        public void ServerCompleteConstructionSystem_WhenRepairSiteFinishes_EmitsFlowFactHandledByStage1FlowOwner()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Stage1SettlementProgressStage.RepairResourcesReady);
            var site = scope.CreateNetworkedConstructionSite(buildWorkDone: 95f);
            var siteGid = site.GID;
            var flowSystem = new ServerStage1FlowSystem();
            flowSystem.Init();

            ref var siteState = ref site.Mut<ConstructionSiteState>();
            ref readonly var siteResources = ref site.Read<ConstructionResources>();
            ref var siteProgress = ref site.Mut<ConstructionProgress>();
            var applied = ConstructionRules.ApplyBuildWork(ref siteState, ref siteProgress, in siteResources, 10f, 10f);
            Assert.That(applied, Is.True);
            Assert.That(siteState.Phase, Is.EqualTo(ConstructionPhase.Completed));

            new ServerCompleteConstructionSystem().Update();
            flowSystem.Update();
            flowSystem.Destroy();

            Assert.That(siteGid.TryUnpack<ServerWT>(out _), Is.False);
            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.CampRepaired));

            var finishedCount = 0;
            foreach (var finished in SW.Query<All<FinishedBuildingTag, SettlementAnchorRef, ConstructionSiteState, ConstructionProgress>>().Entities())
            {
                finishedCount++;
                Assert.That(finished.Read<SettlementAnchorRef>().AnchorId, Is.EqualTo(SettlementAnchorCatalog.HomeCampId.Value));
                Assert.That(finished.Read<ConstructionSiteState>().Phase, Is.EqualTo(ConstructionPhase.Completed));
                Assert.That(finished.Read<ConstructionProgress>().IsComplete, Is.True);
            }

            Assert.That(finishedCount, Is.EqualTo(1));
        }

        [Test]
        public void ServerStage1FlowSystem_WhenAnchorSpawns_AdvancesToRepairObjectiveActive()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Stage1SettlementProgressStage.DamagedCampStart);

            var flowSystem = new ServerStage1FlowSystem();
            flowSystem.Init();
            flowSystem.Update();
            flowSystem.Destroy();

            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.RepairObjectiveActive));
        }

        [Test]
        public void ServerStage1CampAnchorSpawnSystem_SpawnsAnchorWithFullRequiredState()
        {
            using var scope = new CombatTestServerWorldScope();
            SW.SetResource(Stage1SettlementSeedManifest.CreateResource());

            new ServerStage1CampAnchorSpawnSystem().Update();

            Assert.That(Stage1SettlementProgressionQuery.TryGetServerAnchor(SettlementAnchorCatalog.HomeCampId, out var anchor), Is.True);
            Assert.That(anchor.Has<Stage1SettlementProgression>(), Is.True);
            Assert.That(anchor.Has<Stage1FlowViewState>(), Is.True);
            Assert.That(anchor.Has<Stage1ProgressionState>(), Is.True);
            Assert.That(anchor.Has<SettlementWorkerSummary>(), Is.True);
            Assert.That(anchor.Has<SettlementCampBuilderJobState>(), Is.True);
            Assert.That(anchor.Has<ExpeditionAvailabilityState>(), Is.True);
            Assert.That(anchor.Has<ActiveExpeditionState>(), Is.True);
            Assert.That(anchor.Has<ThreatState>(), Is.True);
            Assert.That(anchor.Has<RaidScheduleState>(), Is.True);
            Assert.That(anchor.Has<BossEncounterState>(), Is.True);
            Assert.That(anchor.Has<BossBuildPreparationState>(), Is.True);
            Assert.That(anchor.Has<BossPreparedBuildSnapshot>(), Is.True);
            Assert.That(anchor.Has<SettlementAnchorLocation>(), Is.True);
        }

        [Test]
        public void ServerStage1FlowSystem_WhenRepairResourcesAreAvailable_AdvancesToRepairResourcesReady()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Stage1SettlementProgressStage.RepairObjectiveActive);
            scope.CreateSettlementSharedResources(wood: 50, stone: 25);
            scope.CreateNetworkedConstructionSite(
                phase: ConstructionPhase.ReadyToBuild,
                woodRequired: 10,
                stoneRequired: 4);

            var flowSystem = new ServerStage1FlowSystem();
            flowSystem.Init();
            flowSystem.Update();
            flowSystem.Destroy();

            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.RepairResourcesReady));
        }

        [Test]
        public void ServerStage1FlowSystem_WithoutRepairCompletedEvent_DoesNotAdvanceToCampRepaired()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Stage1SettlementProgressStage.RepairResourcesReady);

            var flowSystem = new ServerStage1FlowSystem();
            flowSystem.Init();
            flowSystem.Update();
            flowSystem.Destroy();

            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.RepairResourcesReady));
        }

        [Test]
        public void SetSettlementWorkerAssignmentHandler_WhenAssignmentAccepted_Stage1FlowOwnerAdvancesToWorkerAssigned()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Stage1SettlementProgressStage.CampRepaired);
            var worker = scope.CreateEntity();
            worker.Set<SettlementWorkerTag>();
            worker.Set(new SettlementWorkerIdentity
            {
                HomeAnchorId = SettlementAnchorCatalog.HomeCampId.Value,
                RoleId = WorkerRoleCatalog.CampBuilderId.Value
            });
            worker.Set(new SettlementWorkerAssignment
            {
                Status = SettlementWorkerAssignmentStatus.Unassigned
            });

            var flowSystem = new ServerStage1FlowSystem();
            flowSystem.Init();
            var result = new SetSettlementWorkerAssignmentHandler().Handle(
                new NetworkPeerId(1),
                new SetSettlementWorkerAssignmentRequestEvent(worker.GID, SettlementAnchorCatalog.HomeCampId, assigned: true));
            flowSystem.Update();
            flowSystem.Destroy();

            Assert.That(result.Status, Is.EqualTo(RequestStatus.Accepted));
            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.WorkerAssigned));
        }

        [Test]
        public void SetSettlementWorkerAssignmentHandler_WhenAssignmentRejected_DoesNotAdvanceFlow()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Stage1SettlementProgressStage.RepairResourcesReady);
            var worker = scope.CreateEntity();
            worker.Set<SettlementWorkerTag>();
            worker.Set(new SettlementWorkerIdentity
            {
                HomeAnchorId = SettlementAnchorCatalog.HomeCampId.Value,
                RoleId = WorkerRoleCatalog.CampBuilderId.Value
            });
            worker.Set(new SettlementWorkerAssignment
            {
                Status = SettlementWorkerAssignmentStatus.Unassigned
            });

            var flowSystem = new ServerStage1FlowSystem();
            flowSystem.Init();
            var result = new SetSettlementWorkerAssignmentHandler().Handle(
                new NetworkPeerId(1),
                new SetSettlementWorkerAssignmentRequestEvent(worker.GID, SettlementAnchorCatalog.HomeCampId, assigned: true));
            flowSystem.Update();
            flowSystem.Destroy();

            Assert.That(result.Status, Is.EqualTo(RequestStatus.Rejected));
            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.RepairResourcesReady));
        }

        [Test]
        public void ServerStage1FlowSystem_WhenBuildPreparedEventReceived_AdvancesToBuildPrepared()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Stage1SettlementProgressStage.WorkerAssigned);

            var flowSystem = new ServerStage1FlowSystem();
            flowSystem.Init();
            SW.SendEvent(new Stage1BuildPreparedEvent(SettlementAnchorCatalog.HomeCampId));
            flowSystem.Update();
            flowSystem.Destroy();

            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.BuildPrepared));
        }

        [Test]
        public void ServerFrontierExpeditionAvailabilitySystem_WhenPreparedBuildSnapshotExists_DoesNotAdvanceStage()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Stage1SettlementProgressStage.WorkerAssigned);
            scope.CreatePlayer(new NetworkPeerId(1), Vector3.zero);

            new ServerPreparedBuildSnapshotSystem().Update();
            new ServerFrontierExpeditionAvailabilitySystem().Update();

            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.WorkerAssigned));
        }

        [Test]
        public void ClientStage1HudStateSystem_WhenCampIsRepaired_ShowsAssignWorkerAndKeepsBuildPreparationClosed()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(stage: Stage1SettlementProgressStage.CampRepaired);
            scope.CreateSharedResources();
            scope.RefreshProjections();

            new ClientStage1HudStateSystem().Update();

            ref readonly var hud = ref CW.GetResource<Stage1HudState>();
            Assert.That(hud.Objective, Is.Not.EqualTo(Stage1ObjectiveKind.RepairCamp));
            Assert.That(hud.Objective, Is.EqualTo(Stage1ObjectiveKind.AssignWorker));
            Assert.That(hud.CanOpenBuildPreparation, Is.False);
            Assert.That(hud.CanOpenExpeditionSelection, Is.False);
        }

        [Test]
        public void Stage1RepairFlow_OneBuildActionDoesNotCompleteInitialCampRepair()
        {
            var state = new ConstructionSiteState
            {
                Phase = ConstructionPhase.ReadyToBuild
            };
            var resources = new ConstructionResources
            {
                WoodRequired = 10,
                WoodDelivered = 10,
                StoneRequired = 4,
                StoneDelivered = 4
            };
            var progress = new ConstructionProgress
            {
                BuildWorkRequired = 100f,
                BuildWorkDone = 0f
            };

            var applied = ConstructionRules.ApplyBuildWork(ref state, ref progress, in resources, 35f / 30f, 5f);

            Assert.That(applied, Is.True);
            Assert.That(state.Phase, Is.EqualTo(ConstructionPhase.BuildingInProgress));
            Assert.That(progress.BuildWorkDone, Is.LessThan(progress.BuildWorkRequired));
            Assert.That(progress.Normalized, Is.LessThan(1f));
        }

        [Test]
        public void ClientStage1HudStateSystem_WhenRepairResourcesAreReady_ShowsContinueBuildingHint()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(stage: Stage1SettlementProgressStage.RepairResourcesReady);
            scope.CreateSharedResources();
            scope.RefreshProjections();

            new ClientStage1HudStateSystem().Update();

            ref readonly var hud = ref CW.GetResource<Stage1HudState>();
            Assert.That(hud.Objective, Is.EqualTo(Stage1ObjectiveKind.RepairCamp));
            Assert.That(hud.ObjectiveHint, Is.EqualTo("Resources delivered. Keep building the camp core to finish repairs."));
        }
    }
}
