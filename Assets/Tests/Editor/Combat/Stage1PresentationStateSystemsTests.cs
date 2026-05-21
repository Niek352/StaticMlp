using System.Reflection;
using System.Threading;
using Code.EcsUi.Mvc;
using Cysharp.Threading.Tasks;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Loadout;
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
            Assert.That(hud.CanOpenLoadoutPreparation, Is.False);
            Assert.That(hud.CanOpenExpeditionSelection, Is.False);
        }

        [Test]
        public void HudState_WhenLoadoutPreparedAndExpeditionAvailable_ShowsStartExpeditionObjective()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(
                stage: Stage1SettlementProgressStage.LoadoutPrepared,
                expeditionAvailability: ExpeditionAvailabilityStatus.Available);
            scope.CreateSharedResources(wood: 70, stone: 30);
            scope.CreateLocalPlayer(LoadoutModuleCatalog.FireFlaskModuleId);
            scope.RefreshProjections();

            new ClientStage1HudStateSystem().Update();

            ref readonly var hud = ref CW.GetResource<Stage1HudState>();
            Assert.That(hud.Objective, Is.EqualTo(Stage1ObjectiveKind.StartExpedition));
            Assert.That(hud.Wood, Is.EqualTo(70));
            Assert.That(hud.Stone, Is.EqualTo(30));
            Assert.That(hud.PreparedPrimaryModuleId, Is.EqualTo(LoadoutModuleCatalog.FireFlaskModuleId));
            Assert.That(hud.ExpeditionAvailability, Is.EqualTo(ExpeditionAvailabilityStatus.Available));
        }

        [TestCase(Stage1SettlementProgressStage.WorkerAssigned, Stage1ObjectiveKind.PlaceStockpile, "Place a stockpile so the settlement can hold expanded resources.")]
        [TestCase(Stage1SettlementProgressStage.StockpilePlaced, Stage1ObjectiveKind.PlaceShelter, "Place a shelter to establish basic worker service.")]
        [TestCase(Stage1SettlementProgressStage.ShelterPlaced, Stage1ObjectiveKind.BringExtractionOnline, "Bring lumber or stone extraction online for steady supply.")]
        [TestCase(Stage1SettlementProgressStage.ExtractionOnline, Stage1ObjectiveKind.BringWorkbenchOnline, "Bring the workbench online to prepare the settlement economy.")]
        public void HudState_WhenEconomyGateIsActive_ShowsSettlementObjectiveAndKeepsLoadoutLocked(
            Stage1SettlementProgressStage stage,
            Stage1ObjectiveKind expectedObjective,
            string expectedHint)
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(stage: stage);
            scope.CreateSharedResources();
            scope.RefreshProjections();

            new ClientStage1HudStateSystem().Update();

            ref readonly var hud = ref CW.GetResource<Stage1HudState>();
            Assert.That(hud.Objective, Is.EqualTo(expectedObjective));
            Assert.That(hud.ObjectiveHint, Is.EqualTo(expectedHint));
            Assert.That(hud.CanOpenLoadoutPreparation, Is.False);
            Assert.That(hud.CanOpenExpeditionSelection, Is.False);
        }

        [Test]
        public void HudState_WhenWorkbenchIsOnline_ShowsPrepareBuildAndUnlocksLoadoutPreparation()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(stage: Stage1SettlementProgressStage.WorkbenchOnline);
            scope.CreateSharedResources();
            scope.RefreshProjections();

            new ClientStage1HudStateSystem().Update();

            ref readonly var hud = ref CW.GetResource<Stage1HudState>();
            Assert.That(hud.Objective, Is.EqualTo(Stage1ObjectiveKind.PrepareBuild));
            Assert.That(hud.ObjectiveHint, Is.Empty);
            Assert.That(hud.CanOpenLoadoutPreparation, Is.True);
        }

        [Test]
        public void HudState_WhenRaidIsPending_ShowsDefendCampObjective()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            var anchor = scope.CreateAnchor(
                stage: Stage1SettlementProgressStage.LoadoutPrepared,
                threatPhase: ThreatPhase.RaidPending,
                raidStatus: RaidScheduleStatus.Pending);
            anchor.Set(new Stage1ProgressionState(SettlementAnchorCatalog.HomeCampId, 0));
            scope.CreateSharedResources();
            scope.CreateLocalPlayer(LoadoutModuleCatalog.PoisonArrowModuleId);
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
            scope.CreateLocalPlayer(LoadoutModuleCatalog.PoisonArrowModuleId);
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
        public void ContextPanelSession_WhenRepairStageIsStaleButAnchorConstructionCompleted_ClearsRepairFocus()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(stage: Stage1SettlementProgressStage.RepairResourcesReady);
            scope.CreateSharedResources();
            scope.CreateConstructionSite(
                ConstructionPhase.Completed,
                woodRequired: 10,
                woodDelivered: 10,
                stoneRequired: 5,
                stoneDelivered: 5,
                progress01: 1f);
            scope.RefreshProjections();

            new ClientStage1PresentationBootstrapSystem().Init();
            new ClientStage1ContextPanelSessionSystem().Update();

            ref readonly var session = ref CW.GetResource<Stage1ContextPanelSession>();
            Assert.That(session.Mode, Is.EqualTo(Stage1ContextPanelMode.Worker));
            Assert.That(session.FocusedSite, Is.EqualTo(default(EntityGID)));
        }

        [Test]
        public void ContextPanel_WhenWorkerModeIsActive_ShowsAssignmentSummary()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(stage: Stage1SettlementProgressStage.CampRepaired);
            scope.CreateSharedResources();
            scope.CreateLocalPlayer(LoadoutModuleCatalog.PoisonArrowModuleId);
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
        public void LoadoutPreparationScreenState_WhenWorkbenchIsOnline_UsesLocalSelectionAndBossCommitGate()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(stage: Stage1SettlementProgressStage.WorkbenchOnline);
            scope.CreateLocalPlayer(LoadoutModuleCatalog.FireFlaskModuleId);
            var bossPreparation = CW.NewEntity<Default>();
            bossPreparation.Set(new BossLoadoutPreparationState
            {
                Status = BossLoadoutPreparationStatus.Committed
            });

            new ClientLoadoutPreparationScreenStateSystem().Update();

            ref readonly var state = ref CW.GetResource<LoadoutPreparationScreenState>();
            Assert.That(state.IsAvailable, Is.True);
            Assert.That(state.SelectedPrimaryModuleId, Is.EqualTo(LoadoutModuleCatalog.FireFlaskModuleId));
            Assert.That(state.FireFlaskSelected, Is.True);
            Assert.That(state.IsBossCommitted, Is.True);
            Assert.That(state.CanConfirm, Is.False);
        }

        [Test]
        public void LoadoutPreparationScreenState_WhenEconomyChainIsIncomplete_StaysClosedFromFlowViewState()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(stage: Stage1SettlementProgressStage.ExtractionOnline);
            scope.CreateLocalPlayer(LoadoutModuleCatalog.FireFlaskModuleId);
            scope.RefreshProjections();

            new ClientLoadoutPreparationScreenStateSystem().Update();

            ref readonly var state = ref CW.GetResource<LoadoutPreparationScreenState>();
            Assert.That(state.IsAvailable, Is.False);
        }

        [Test]
        public void LoadoutPreparationController_WhenConfirmInvoked_SendsPrepareLoadoutCommand()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(stage: Stage1SettlementProgressStage.WorkbenchOnline);
            scope.CreateLocalPlayer(LoadoutModuleCatalog.FireFlaskModuleId);
            CW.SetResource(new NetOutbox());
            NetworkRuntime.LocalPeerId = new NetworkPeerId(1);

            new ClientLoadoutPresentationBootstrapSystem().Init();
            new ClientLoadoutPreparationScreenStateSystem().Update();

            var controller = new LoadoutPreparationController(
                () => null,
                new ControllerResourceBridgeSystem<LoadoutPreparationController, LoadoutPreparationScreenState>());
            var sendSystem = new ClientNetworkEventSendSystem();
            sendSystem.Init();

            InvokePrivate(controller, "ConfirmBuild");
            new ClientLoadoutSelectionSystem().Update();
            sendSystem.Update();

            foreach (var player in CW.Query<All<ClientLoadoutSelectionSyncState>>().Entities())
            {
                Assert.That(player.Read<ClientLoadoutSelectionSyncState>().LastSentPrimaryModuleId, Is.EqualTo(LoadoutModuleCatalog.FireFlaskModuleId));
                Assert.That(player.Read<ClientLoadoutSelectionSyncState>().ShouldCommitSelection, Is.False);
                break;
            }

            ref var outbox = ref CW.GetResource<NetOutbox>();
            outbox.FlushNetworkEventBatches();
            Assert.That(outbox.Packets, Has.Count.EqualTo(1));
            sendSystem.Destroy();
        }

        [Test]
        public void LoadoutPreparationController_WhenBossPreparationIsAvailable_SendsPrepareBossRequest()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            var anchor = scope.CreateAnchor(stage: Stage1SettlementProgressStage.LoadoutPrepared);
            ref var progression = ref anchor.Mut<Stage1ProgressionState>();
            progression.ApplyFlag(ProgressFlagCatalog.CounterattackDefendedId);
            progression.GrantBossPreparationTokens(1);
            scope.CreateLocalPlayer(LoadoutModuleCatalog.FireFlaskModuleId);
            scope.RefreshProjections();
            CW.SetResource(new NetOutbox());
            NetworkRuntime.LocalPeerId = new NetworkPeerId(1);

            new ClientLoadoutPresentationBootstrapSystem().Init();
            new ClientLoadoutPreparationScreenStateSystem().Update();

            var controller = new LoadoutPreparationController(
                () => null,
                new ControllerResourceBridgeSystem<LoadoutPreparationController, LoadoutPreparationScreenState>());
            var sendSystem = new ClientNetworkEventSendSystem();
            sendSystem.Init();

            InvokePrivate(controller, "ConfirmBuild");
            sendSystem.Update();

            ref readonly var state = ref CW.GetResource<LoadoutPreparationScreenState>();
            Assert.That(state.CanPrepareBoss, Is.True);
            ref var outbox = ref CW.GetResource<NetOutbox>();
            outbox.FlushNetworkEventBatches();
            Assert.That(outbox.Packets, Has.Count.EqualTo(1));
            sendSystem.Destroy();
        }

        [Test]
        public void ExpeditionSelectionScreenState_ExposesPreparedBuildAndRewardPreview()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(
                stage: Stage1SettlementProgressStage.LoadoutPrepared,
                expeditionAvailability: ExpeditionAvailabilityStatus.Available);
            scope.CreateLocalPlayer(LoadoutModuleCatalog.FireFlaskModuleId);
            scope.RefreshProjections();

            new ClientExpeditionSelectionScreenStateSystem().Update();

            ref readonly var state = ref CW.GetResource<ExpeditionSelectionScreenState>();
            Assert.That(state.CanStart, Is.True);
            Assert.That(state.PreparedPrimaryModuleId, Is.EqualTo(LoadoutModuleCatalog.FireFlaskModuleId));
            Assert.That(state.RewardPackageId, Is.EqualTo(RewardPackageCatalog.RecoveredWarCacheId));
        }

        [Test]
        public void ExpeditionSelectionController_WhenStartInvoked_SendsStartExpeditionRequest()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(
                stage: Stage1SettlementProgressStage.LoadoutPrepared,
                expeditionAvailability: ExpeditionAvailabilityStatus.Available);
            scope.CreateLocalPlayer(LoadoutModuleCatalog.FireFlaskModuleId);
            scope.RefreshProjections();
            CW.SetResource(new NetOutbox());
            NetworkRuntime.LocalPeerId = new NetworkPeerId(1);

            new ClientFrontierPresentationBootstrapSystem().Init();
            new ClientExpeditionSelectionScreenStateSystem().Update();

            var controller = new ExpeditionSelectionController(
                () => null,
                new ControllerResourceBridgeSystem<ExpeditionSelectionController, ExpeditionSelectionScreenState>());
            var sendSystem = new ClientNetworkEventSendSystem();
            sendSystem.Init();

            InvokePrivate(controller, "StartExpedition");
            sendSystem.Update();

            ref var outbox = ref CW.GetResource<NetOutbox>();
            outbox.FlushNetworkEventBatches();
            Assert.That(outbox.Packets, Has.Count.EqualTo(1));
            sendSystem.Destroy();
        }

        [Test]
        public void ExpeditionSelectionController_WhenBossIsAvailable_SendsStartBossRequest()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(
                stage: Stage1SettlementProgressStage.LoadoutPrepared,
                bossStatus: BossEncounterStatus.Available);
            scope.CreateLocalPlayer(LoadoutModuleCatalog.FireFlaskModuleId);
            scope.RefreshProjections();
            CW.SetResource(new NetOutbox());
            NetworkRuntime.LocalPeerId = new NetworkPeerId(1);

            new ClientFrontierPresentationBootstrapSystem().Init();
            new ClientExpeditionSelectionScreenStateSystem().Update();

            var controller = new ExpeditionSelectionController(
                () => null,
                new ControllerResourceBridgeSystem<ExpeditionSelectionController, ExpeditionSelectionScreenState>());
            var sendSystem = new ClientNetworkEventSendSystem();
            sendSystem.Init();

            InvokePrivate(controller, "StartExpedition");
            sendSystem.Update();

            ref readonly var state = ref CW.GetResource<ExpeditionSelectionScreenState>();
            Assert.That(state.IsBossEncounterMode, Is.True);
            Assert.That(state.CanStart, Is.True);
            ref var outbox = ref CW.GetResource<NetOutbox>();
            outbox.FlushNetworkEventBatches();
            Assert.That(outbox.Packets, Has.Count.EqualTo(1));
            sendSystem.Destroy();
        }

        [Test]
        public void RewardPopup_WhenNewRewardAppears_OpensExactlyOnce()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            var anchor = scope.CreateAnchor(stage: Stage1SettlementProgressStage.LoadoutPrepared);
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

        [Test]
        public void ThreatBannerBridge_WhenStateSystemRunsBeforeSync_AppliesFreshThreatState()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(
                stage: Stage1SettlementProgressStage.LoadoutPrepared,
                threatPhase: ThreatPhase.RaidPending,
                raidStatus: RaidScheduleStatus.Pending);
            scope.RefreshProjections();

            new ClientFrontierPresentationBootstrapSystem().Init();
            var controller = new TestThreatBannerController { State = ControllerState.ViewFocused };
            var bridge = new ControllerResourceBridgeSystem<TestThreatBannerController, ThreatBannerState>();
            bridge.Bind(controller);
            bridge.Activate();

            new ClientThreatBannerStateSystem().Update();
            bridge.Update();

            Assert.That(controller.Phase, Is.EqualTo(ThreatPhase.RaidPending));
            Assert.That(controller.RaidStatus, Is.EqualTo(RaidScheduleStatus.Pending));
            Assert.That(controller.ActivateAtTick, Is.EqualTo(77));
        }

        private static void InvokePrivate(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Expected private method '{methodName}' on {target.GetType().Name}.");
            method.Invoke(target, null);
        }

        private sealed class TestThreatBannerController : IController, IResourcePresentationController<ThreatBannerState>
        {
            public ControllerState State { get; set; }
            public ViewLayer Layer => ViewLayer.Persistent;
            public int? PersistentSortOrder => 200;
            public bool CanBeClosedByEscape => false;
            public ThreatPhase Phase { get; private set; }
            public RaidScheduleStatus RaidStatus { get; private set; }
            public uint ActivateAtTick { get; private set; }

            public void Apply(in ThreatBannerState state)
            {
                Phase = state.Phase;
                RaidStatus = state.RaidStatus;
                ActivateAtTick = state.ActivateAtTick;
            }

            public void Dispose()
            {
            }

            public void Focus()
            {
                State = ControllerState.ViewFocused;
            }

            public void Blur()
            {
                State = ControllerState.ViewBlurred;
            }

            public UniTask HideViewAsync(CancellationToken ct)
            {
                State = ControllerState.ViewHidden;
                return UniTask.CompletedTask;
            }

            public void SetViewPresentationActive(bool isActive)
            {
            }

            public void RequestClose()
            {
            }
        }
    }
}
