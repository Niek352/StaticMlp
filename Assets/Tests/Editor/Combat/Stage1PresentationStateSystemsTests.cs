using System.Reflection;
using Code.EcsUi.Mvc;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Build;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Tests.Combat
{
    public sealed class Stage1PresentationStateSystemsTests
    {
        [Test]
        public void HudState_WhenRepairObjectiveIsActive_UsesSharedSettlementResourcesAndKeepsProgressionGatesClosed()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(stage: Stage1SettlementProgressStage.RepairObjectiveActive);
            scope.CreateSharedResources(wood: 50, stone: 25);
            scope.RefreshProjections();

            new ClientStage1HudStateSystem().Update();

            ref readonly var hud = ref CW.GetResource<Stage1HudState>();
            Assert.That(hud.Objective, Is.EqualTo(Stage1ObjectiveKind.RepairCamp));
            Assert.That(hud.Wood, Is.EqualTo(50));
            Assert.That(hud.Stone, Is.EqualTo(25));
            Assert.That(hud.CanOpenBuildPreparation, Is.False);
            Assert.That(hud.CanOpenExpeditionSelection, Is.False);
        }

        [Test]
        public void HudState_WhenBuildPreparedAndExpeditionAvailable_ShowsStartExpeditionObjective()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(
                stage: Stage1SettlementProgressStage.BuildPrepared,
                expeditionAvailability: ExpeditionAvailabilityStatus.Available);
            scope.CreateSharedResources(wood: 70, stone: 30);
            scope.CreateLocalPlayer(BuildModuleCatalog.FireFlaskModuleId);
            scope.RefreshProjections();

            new ClientStage1HudStateSystem().Update();

            ref readonly var hud = ref CW.GetResource<Stage1HudState>();
            Assert.That(hud.Objective, Is.EqualTo(Stage1ObjectiveKind.StartExpedition));
            Assert.That(hud.Wood, Is.EqualTo(70));
            Assert.That(hud.Stone, Is.EqualTo(30));
            Assert.That(hud.PreparedPrimaryModuleId, Is.EqualTo(BuildModuleCatalog.FireFlaskModuleId));
            Assert.That(hud.ExpeditionAvailability, Is.EqualTo(ExpeditionAvailabilityStatus.Available));
        }

        [Test]
        public void HudState_WhenRaidIsPending_ShowsDefendCampObjective()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            var anchor = scope.CreateAnchor(
                stage: Stage1SettlementProgressStage.BuildPrepared,
                threatPhase: ThreatPhase.RaidPending,
                raidStatus: RaidScheduleStatus.Pending);
            anchor.Set(new Stage1ProgressionState(SettlementAnchorCatalog.HomeCampId, 0));
            scope.CreateSharedResources();
            scope.CreateLocalPlayer(BuildModuleCatalog.PoisonArrowModuleId);
            scope.RefreshProjections();

            new ClientStage1HudStateSystem().Update();
            new ClientThreatBannerStateSystem().Update();

            ref readonly var hud = ref CW.GetResource<Stage1HudState>();
            ref readonly var banner = ref CW.GetResource<ThreatBannerState>();
            Assert.That(hud.Objective, Is.EqualTo(Stage1ObjectiveKind.DefendCamp));
            Assert.That(banner.Phase, Is.EqualTo(ThreatPhase.RaidPending));
            Assert.That(banner.RaidStatus, Is.EqualTo(RaidScheduleStatus.Pending));
            Assert.That(banner.ActivateAtTick, Is.EqualTo(77));
        }

        [Test]
        public void ContextPanel_WhenBuildingIsFocused_ExposesConstructionActions()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(stage: Stage1SettlementProgressStage.CampRepaired);
            scope.CreateSharedResources();
            scope.CreateLocalPlayer(BuildModuleCatalog.PoisonArrowModuleId);
            var site = scope.CreateConstructionSite(
                ConstructionPhase.ReadyToBuild,
                woodRequired: 10,
                woodDelivered: 10,
                stoneRequired: 5,
                stoneDelivered: 5,
                progress01: 0.4f);
            scope.RefreshProjections();

            new ClientStage1PresentationBootstrapSystem().Init();
            ref var session = ref CW.GetResource<Stage1ContextPanelSession>();
            session.Mode = Stage1ContextPanelMode.Building;
            session.FocusedSite = site.GID;
            new ClientStage1ContextPanelStateSystem().Update();

            ref readonly var state = ref CW.GetResource<Stage1ContextPanelState>();
            Assert.That(state.Mode, Is.EqualTo(Stage1ContextPanelMode.Building));
            Assert.That(state.HasFocusedSite, Is.True);
            Assert.That(state.CanDepositResources, Is.False);
            Assert.That(state.CanBuild, Is.True);
            Assert.That(state.Progress01, Is.EqualTo(0.4f));
        }

        [Test]
        public void ContextPanelSession_WhenRepairObjectiveIsActive_FocusesAnchoredRepairSiteWithoutPlayer()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(stage: Stage1SettlementProgressStage.DamagedCampStart);
            scope.CreateSharedResources();
            var site = scope.CreateConstructionSite(
                ConstructionPhase.ReadyToBuild,
                woodRequired: 10,
                woodDelivered: 10,
                stoneRequired: 5,
                stoneDelivered: 5,
                progress01: 0.2f);
            site.Set(new Stage1SettlementProgression
            {
                AnchorId = SettlementAnchorCatalog.HomeCampId.Value,
                Stage = Stage1SettlementProgressStage.DamagedCampStart
            });
            scope.RefreshProjections();

            new ClientStage1PresentationBootstrapSystem().Init();
            new ClientStage1ContextPanelSessionSystem().Update();
            new ClientStage1ContextPanelStateSystem().Update();

            ref readonly var session = ref CW.GetResource<Stage1ContextPanelSession>();
            ref readonly var state = ref CW.GetResource<Stage1ContextPanelState>();
            Assert.That(session.Mode, Is.EqualTo(Stage1ContextPanelMode.Building));
            Assert.That(session.FocusedSite, Is.EqualTo(site.GID));
            Assert.That(state.Mode, Is.EqualTo(Stage1ContextPanelMode.Building));
            Assert.That(state.CanDepositResources, Is.False);
            Assert.That(state.CanBuild, Is.True);
        }

        [Test]
        public void ContextPanel_WhenWorkerModeIsActive_ShowsAssignmentSummary()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(stage: Stage1SettlementProgressStage.CampRepaired);
            scope.CreateSharedResources();
            scope.CreateLocalPlayer(BuildModuleCatalog.PoisonArrowModuleId);
            scope.CreateWorker(
                assigned: false,
                blockingReason: SettlementWorkerBlockingReason.NoAssignment,
                activeTask: AiTaskType.Idle);
            scope.RefreshProjections();

            new ClientStage1PresentationBootstrapSystem().Init();
            ref var session = ref CW.GetResource<Stage1ContextPanelSession>();
            session.Mode = Stage1ContextPanelMode.Worker;
            new ClientStage1ContextPanelStateSystem().Update();

            ref readonly var state = ref CW.GetResource<Stage1ContextPanelState>();
            Assert.That(state.Mode, Is.EqualTo(Stage1ContextPanelMode.Worker));
            Assert.That(state.HasWorker, Is.True);
            Assert.That(state.WorkerAssigned, Is.False);
            Assert.That(state.WorkerBlockingReason, Is.EqualTo(SettlementWorkerBlockingReason.NoAssignment));
            Assert.That(state.CanToggleWorkerAssignment, Is.True);
        }

        [Test]
        public void BuildPreparationScreenState_UsesLocalSelectionAndBossCommitGate()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(stage: Stage1SettlementProgressStage.CampRepaired);
            scope.CreateLocalPlayer(BuildModuleCatalog.FireFlaskModuleId);
            var bossPreparation = CW.NewEntity<Default>();
            bossPreparation.Set(new BossBuildPreparationState
            {
                Status = BossBuildPreparationStatus.Committed
            });

            new ClientBuildPreparationScreenStateSystem().Update();

            ref readonly var state = ref CW.GetResource<BuildPreparationScreenState>();
            Assert.That(state.IsAvailable, Is.True);
            Assert.That(state.SelectedPrimaryModuleId, Is.EqualTo(BuildModuleCatalog.FireFlaskModuleId));
            Assert.That(state.FireFlaskSelected, Is.True);
            Assert.That(state.IsBossCommitted, Is.True);
            Assert.That(state.CanConfirm, Is.False);
        }

        [Test]
        public void BuildPreparationController_WhenConfirmInvoked_SendsPrepareBuildCommand()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(stage: Stage1SettlementProgressStage.CampRepaired);
            scope.CreateLocalPlayer(BuildModuleCatalog.FireFlaskModuleId);
            CW.SetResource(new NetOutbox());
            NetworkRuntime.LocalPeerId = new NetworkPeerId(1);

            var controller = new BuildPreparationController(
                () => null,
                new ControllerResourceBridgeSystem<BuildPreparationController, BuildPreparationScreenState>((_, _) => { }));
            var sendSystem = new ClientNetworkEventSendSystem();
            sendSystem.Init();

            InvokePrivate(controller, "ConfirmBuild");
            new ClientBuildSelectionSystem().Update();
            sendSystem.Update();

            foreach (var player in CW.Query<All<ClientBuildSelectionSyncState>>().Entities())
            {
                Assert.That(player.Read<ClientBuildSelectionSyncState>().LastSentPrimaryModuleId, Is.EqualTo(BuildModuleCatalog.FireFlaskModuleId));
                Assert.That(player.Read<ClientBuildSelectionSyncState>().ShouldCommitSelection, Is.False);
                break;
            }

            Assert.That(CW.GetResource<NetOutbox>().NetworkEventPackets, Has.Count.EqualTo(1));
            sendSystem.Destroy();
        }

        [Test]
        public void ExpeditionSelectionScreenState_ExposesPreparedBuildAndRewardPreview()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(
                stage: Stage1SettlementProgressStage.BuildPrepared,
                expeditionAvailability: ExpeditionAvailabilityStatus.Available);
            scope.CreateLocalPlayer(BuildModuleCatalog.FireFlaskModuleId);
            scope.RefreshProjections();

            new ClientExpeditionSelectionScreenStateSystem().Update();

            ref readonly var state = ref CW.GetResource<ExpeditionSelectionScreenState>();
            Assert.That(state.CanStart, Is.True);
            Assert.That(state.PreparedPrimaryModuleId, Is.EqualTo(BuildModuleCatalog.FireFlaskModuleId));
            Assert.That(state.RewardPackageId, Is.EqualTo(RewardPackageCatalog.RecoveredWarCacheId));
        }

        [Test]
        public void ExpeditionSelectionController_WhenStartInvoked_SendsStartExpeditionRequest()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(
                stage: Stage1SettlementProgressStage.BuildPrepared,
                expeditionAvailability: ExpeditionAvailabilityStatus.Available);
            scope.CreateLocalPlayer(BuildModuleCatalog.FireFlaskModuleId);
            scope.RefreshProjections();
            CW.SetResource(new NetOutbox());
            NetworkRuntime.LocalPeerId = new NetworkPeerId(1);

            new ClientFrontierPresentationBootstrapSystem().Init();
            new ClientExpeditionSelectionScreenStateSystem().Update();

            var controller = new ExpeditionSelectionController(
                () => null,
                new ControllerResourceBridgeSystem<ExpeditionSelectionController, ExpeditionSelectionScreenState>((_, _) => { }));
            var sendSystem = new ClientNetworkEventSendSystem();
            sendSystem.Init();

            InvokePrivate(controller, "StartExpedition");
            sendSystem.Update();

            Assert.That(CW.GetResource<NetOutbox>().NetworkEventPackets, Has.Count.EqualTo(1));
            sendSystem.Destroy();
        }

        [Test]
        public void RewardPopup_WhenNewRewardAppears_OpensExactlyOnce()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            var anchor = scope.CreateAnchor(stage: Stage1SettlementProgressStage.BuildPrepared);
            anchor.Set(new Stage1ProgressionState(SettlementAnchorCatalog.HomeCampId, 0));
            ref var progression = ref anchor.Mut<Stage1ProgressionState>();
            progression.MarkRewardApplied(RewardPackageCatalog.RecoveredWarCacheId);
            progression.ApplyFlag(ProgressFlagCatalog.RecoveredWarCacheAppliedId);
            scope.RefreshProjections();

            new ClientProgressionPresentationBootstrapSystem().Init();
            new ClientRewardResultPopupStateSystem().Update();

            ref var popup = ref CW.GetResource<RewardResultPopupState>();
            Assert.That(popup.IsVisible, Is.True);
            Assert.That(popup.RewardPackageId, Is.EqualTo(RewardPackageCatalog.RecoveredWarCacheId));
            Assert.That(popup.GrantedWood, Is.EqualTo(20));
            Assert.That(popup.GrantedStone, Is.EqualTo(10));
            Assert.That(popup.ThreatRaised, Is.True);

            popup.IsVisible = false;
            popup.LastPresentedRewardsMask = popup.CurrentAppliedRewardsMask;
            new ClientRewardResultPopupStateSystem().Update();
            Assert.That(CW.GetResource<RewardResultPopupState>().IsVisible, Is.False);
        }

        private static void InvokePrivate(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Expected private method '{methodName}' on {target.GetType().Name}.");
            method.Invoke(target, null);
        }
    }
}
