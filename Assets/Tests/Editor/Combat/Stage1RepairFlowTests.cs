using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Tests.Combat
{
    public sealed class Stage1RepairFlowTests
    {
        [Test]
        public void ServerCompleteConstructionSystem_WhenRepairSiteFinishes_AdvancesAnchorToCampRepaired()
        {
            using var scope = new CombatTestServerWorldScope();
            var site = scope.CreateNetworkedConstructionSite(buildWorkDone: 95f);
            var siteGid = site.GID;

            ref var siteState = ref site.Mut<ConstructionSiteState>();
            ref readonly var siteResources = ref site.Read<ConstructionResources>();
            ref var siteProgress = ref site.Mut<ConstructionProgress>();
            var applied = ConstructionRules.ApplyBuildWork(ref siteState, ref siteProgress, in siteResources, 10f, 10f);
            Assert.That(applied, Is.True);
            Assert.That(siteState.Phase, Is.EqualTo(ConstructionPhase.Completed));

            new ServerCompleteConstructionSystem().Update();

            Assert.That(siteGid.TryUnpack<ServerWT>(out _), Is.False);

            var finishedCount = 0;
            foreach (var finished in SW.Query<All<FinishedBuildingTag, Stage1SettlementProgression, ConstructionSiteState, ConstructionProgress>>().Entities())
            {
                finishedCount++;
                Assert.That(finished.Read<Stage1SettlementProgression>().AnchorId, Is.EqualTo(SettlementAnchorCatalog.HomeCampId.Value));
                Assert.That(finished.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.CampRepaired));
                Assert.That(finished.Read<ConstructionSiteState>().Phase, Is.EqualTo(ConstructionPhase.Completed));
                Assert.That(finished.Read<ConstructionProgress>().IsComplete, Is.True);
            }

            Assert.That(finishedCount, Is.EqualTo(1));
        }

        [Test]
        public void ClientStage1HudStateSystem_WhenCampIsRepaired_ShowsAssignWorkerAndUnlocksBuildPreparation()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(stage: Stage1SettlementProgressStage.CampRepaired);
            scope.CreateSharedResources();
            scope.RefreshProjections();

            new ClientStage1HudStateSystem().Update();

            ref readonly var hud = ref CW.GetResource<Stage1HudState>();
            Assert.That(hud.Objective, Is.Not.EqualTo(Stage1ObjectiveKind.RepairCamp));
            Assert.That(hud.Objective, Is.EqualTo(Stage1ObjectiveKind.AssignWorker));
            Assert.That(hud.CanOpenBuildPreparation, Is.True);
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
