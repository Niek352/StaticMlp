using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Player;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Features.Stage1;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Replication.Generated;
using StaticMlp.Networking.Requests;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class Stage1PresentationClientWorldScope : IDisposable
    {
        public Stage1PresentationClientWorldScope()
        {
            if (CW.Status != WorldStatus.NotCreated)
                CW.Destroy();

            NetworkRuntime.LocalPeerId = new NetworkPeerId(1);
            NetworkEventRegistry.Clear();
            ReplicatedNetworkEventRegistry.RegisterNetworkEvents();
            RequestRegistry.Clear();
            ProjectionRegistry.Clear();

            new SettlementSharedResourcesGameplayFeature().RegisterNetworkEvents();
            new SettlementWorkersGameplayFeature().RegisterNetworkEvents();
            new LoadoutLogicFeature().RegisterNetworkEvents();
            new BuildingsGameplayFeature().RegisterNetworkEvents();
            new FrontierLogicFeature().RegisterNetworkEvents();
            new ProgressionLogicFeature().RegisterNetworkEvents();
            new Stage1CampAnchorGameplayFeature().RegisterNetworkEvents();


            CW.Create(WorldConfig.Default());
            CW.Types().RegisterAll(
                typeof(ClientCoreWT).Assembly,
                typeof(SettlementAnchorRef).Assembly,
                typeof(SettlementSharedResourcesGameplayFeature).Assembly,
                typeof(SettlementPresentationFeature).Assembly,
                typeof(SettlementWorkersGameplayFeature).Assembly,
                typeof(LoadoutLogicFeature).Assembly,
                typeof(LoadoutPresentationFeature).Assembly,
                typeof(BuildingsGameplayFeature).Assembly,
                typeof(FrontierLogicFeature).Assembly,
                typeof(FrontierPresentationFeature).Assembly,
                typeof(ProgressionLogicFeature).Assembly,
                typeof(ProgressionPresentationFeature).Assembly,
                typeof(Stage1FlowViewState).Assembly,
                typeof(Stage1CampAnchorGameplayFeature).Assembly,
                typeof(PlayerTag).Assembly);
            ProjectionRegistry.RegisterClientWorldTypes();
            NetworkEventRegistry.RegisterClientWorldTypes();
            CW.Initialize();
            CW.SetResource(new NetOutbox());
        }

        public CW.Entity CreateAnchor(
            Stage1SettlementProgressStage stage = Stage1SettlementProgressStage.DamagedCampStart,
            ThreatPhase threatPhase = ThreatPhase.Calm,
            RaidScheduleStatus raidStatus = RaidScheduleStatus.None,
            ExpeditionAvailabilityStatus expeditionAvailability = ExpeditionAvailabilityStatus.Unavailable,
            ExpeditionActivityStatus expeditionActivity = ExpeditionActivityStatus.None,
            BossEncounterStatus bossStatus = BossEncounterStatus.Unavailable)
        {
            var anchor = CW.NewEntity<Default>();
            anchor.Set(new Stage1SettlementProgression
            {
                AnchorId = SettlementAnchorCatalog.HomeCampId.Value,
                Stage = stage
            });
            anchor.Set(new Stage1ProgressionState(SettlementAnchorCatalog.HomeCampId, 0));
            anchor.Set(CreateFlowViewState(stage, expeditionAvailability, expeditionActivity, threatPhase, raidStatus, bossStatus));
            anchor.Set(new SettlementWorkerSummary
            {
                AnchorId = SettlementAnchorCatalog.HomeCampId.Value,
                BlockingReason = SettlementWorkerBlockingReason.NoAssignment
            });
            anchor.Set(new SettlementCampBuilderJobState
            {
                AnchorId = SettlementAnchorCatalog.HomeCampId.Value,
                BlockingReason = SettlementWorkerBlockingReason.NoAssignment
            });
            anchor.Set(new ExpeditionAvailabilityState
            {
                ExpeditionIdValue = ExpeditionCatalog.NearbyRaiderCampId.Value,
                Status = expeditionAvailability
            });
            anchor.Set(new ActiveExpeditionState
            {
                ExpeditionIdValue = ExpeditionCatalog.NearbyRaiderCampId.Value,
                Status = expeditionActivity
            });
            anchor.Set(new ThreatState
            {
                Phase = threatPhase,
                ThreatValue = 1
            });
            anchor.Set(new RaidScheduleState
            {
                RaidIdValue = RaidCatalog.RaiderCounterattackId.Value,
                Status = raidStatus,
                ActivateAtTick = 77
            });
            anchor.Set(new BossEncounterState
            {
                BossIdValue = BossCatalog.RaiderChiefId.Value,
                Status = bossStatus
            });
            return anchor;
        }

        public CW.Entity CreateSharedResources(int wood = 50, int stone = 25)
        {
            var entity = CW.NewEntity<Default>();
            entity.Set<SettlementResourceStorageTag>();
            entity.Set(new SettlementSharedResources { Capacity = int.MaxValue });
            ref var rows = ref entity.Add<CW.Multi<SettlementStoredResource>>();
            rows.Add(new SettlementStoredResource(ResourceCatalog.WoodId, wood));
            rows.Add(new SettlementStoredResource(ResourceCatalog.StoneId, stone));
            rows.Add(new SettlementStoredResource(ResourceCatalog.PlanksId, 0));
            rows.Add(new SettlementStoredResource(ResourceCatalog.SimplePartsId, 0));
            rows.Add(new SettlementStoredResource(ResourceCatalog.RepairKitsId, 0));
            rows.Add(new SettlementStoredResource(ResourceCatalog.FoodId, 0));
            rows.Add(new SettlementStoredResource(ResourceCatalog.FuelId, 0));
            rows.Add(new SettlementStoredResource(ResourceCatalog.ResearchDataId, 0));
            rows.Add(new SettlementStoredResource(ResourceCatalog.MedicineId, 0));
            return entity;
        }

        public CW.Entity CreateLocalPlayer(LoadoutModuleId moduleId)
        {
            var player = CW.NewEntity<Default>();
            player.Set<LocalOwned>();
            player.Set<PlayerTag>();
            player.Set(new CharacterNetState
            {
                Position = Vector3.zero,
                Rotation = Quaternion.identity
            });
            player.Set(new OwnerLoadoutSelection
            {
                PrimaryModuleId = moduleId
            });
            player.Set(Stage1LoadoutRules.CreatePreparedSnapshot(player.Read<OwnerLoadoutSelection>()));
            player.Set(new ClientLoadoutSelectionSyncState());
            return player;
        }

        public CW.Entity CreateConstructionSite(ConstructionPhase phase, int woodRequired, int woodDelivered, int stoneRequired, int stoneDelivered, float progress01)
        {
            var site = CW.NewEntity<Default>();
            site.Set<ConstructionSiteTag>();
            site.Set(new ConstructionTransform
            {
                Position = new Vector3(0f, 0f, 1f),
                Rotation = Quaternion.identity
            });
            site.Set(new ConstructionSiteState
            {
                Phase = phase
            });
            site.Set(new ConstructionResources());
            ref var rows = ref site.Add<CW.Multi<ConstructionResourceEntry>>();
            rows.Add(new ConstructionResourceEntry(ResourceCatalog.WoodId, woodRequired, woodDelivered));
            rows.Add(new ConstructionResourceEntry(ResourceCatalog.StoneId, stoneRequired, stoneDelivered));
            site.Set(new SettlementAnchorRef(SettlementAnchorCatalog.HomeCampId));
            site.Set(new ConstructionProgress
            {
                BuildWorkRequired = 1f,
                BuildWorkDone = progress01
            });
            return site;
        }

        public CW.Entity CreateWorker(bool assigned, SettlementWorkerBlockingReason blockingReason, AiTaskType activeTask = AiTaskType.Idle)
        {
            var worker = CW.NewEntity<Default>();
            worker.Set<SettlementWorkerTag>();
            worker.Set(new SettlementWorkerIdentity
            {
                HomeAnchorId = SettlementAnchorCatalog.HomeCampId.Value,
                RoleId = WorkerRoleCatalog.CampBuilderId.Value
            });
            worker.Set(new SettlementWorkerAssignment
            {
                AnchorId = assigned ? SettlementAnchorCatalog.HomeCampId.Value : (ushort)0,
                Status = assigned ? SettlementWorkerAssignmentStatus.Assigned : SettlementWorkerAssignmentStatus.Unassigned
            });
            worker.Set(new CharacterNetState
            {
                Position = new Vector3(0f, 0f, 2f),
                Rotation = Quaternion.identity
            });

            foreach (var anchor in CW.Query<All<Stage1SettlementProgression>>().Entities())
            {
                anchor.Set(new SettlementWorkerSummary
                {
                    AnchorId = SettlementAnchorCatalog.HomeCampId.Value,
                    TotalWorkers = 1,
                    AssignedWorkers = assigned ? (ushort)1 : (ushort)0,
                    CampBuilderWorkers = 1,
                    CampBuilderAssignedWorkers = assigned ? (ushort)1 : (ushort)0,
                    ActiveTask = activeTask,
                    BlockingReason = blockingReason
                });
                anchor.Set(new SettlementCampBuilderJobState
                {
                    AnchorId = SettlementAnchorCatalog.HomeCampId.Value,
                    AssignedWorker = assigned ? worker.GID : default,
                    CurrentTask = activeTask,
                    BlockingReason = blockingReason
                });
                break;
            }

            return worker;
        }

        public void RefreshProjections()
        {
            ProjectionRegistry.Rebuild();
        }

        private static Stage1FlowViewState CreateFlowViewState(
            Stage1SettlementProgressStage stage,
            ExpeditionAvailabilityStatus expeditionAvailability,
            ExpeditionActivityStatus expeditionActivity,
            ThreatPhase threatPhase,
            RaidScheduleStatus raidStatus,
            BossEncounterStatus bossStatus)
        {
            return new Stage1FlowViewState
            {
                AnchorId = SettlementAnchorCatalog.HomeCampId.Value,
                Stage = stage,
                Objective = ResolveObjective(stage, expeditionAvailability, expeditionActivity, threatPhase, bossStatus),
                Hint = ResolveHint(stage),
                CanToggleWorkerAssignment = stage >= Stage1SettlementProgressStage.CampRepaired,
                CanOpenLoadoutPreparation = stage >= Stage1SettlementProgressStage.WorkbenchOnline
                                         && bossStatus != BossEncounterStatus.Active
                                         && bossStatus != BossEncounterStatus.Defeated,
                CanOpenExpeditionSelection = (expeditionAvailability == ExpeditionAvailabilityStatus.Available
                                             || bossStatus == BossEncounterStatus.Available)
                                             && expeditionActivity == ExpeditionActivityStatus.None
                                             && threatPhase != ThreatPhase.RaidPending
                                             && threatPhase != ThreatPhase.RaidActive
                                             && raidStatus == RaidScheduleStatus.None
            };
        }

        private static Stage1FlowObjective ResolveObjective(
            Stage1SettlementProgressStage stage,
            ExpeditionAvailabilityStatus expeditionAvailability,
            ExpeditionActivityStatus expeditionActivity,
            ThreatPhase threatPhase,
            BossEncounterStatus bossStatus)
        {
            if (bossStatus == BossEncounterStatus.Defeated)
                return Stage1FlowObjective.VerticalSliceComplete;

            if (bossStatus == BossEncounterStatus.Active)
                return Stage1FlowObjective.DefeatBoss;

            if (bossStatus == BossEncounterStatus.Available)
                return Stage1FlowObjective.StartBossEncounter;

            if (threatPhase == ThreatPhase.RaidPending || threatPhase == ThreatPhase.RaidActive)
                return Stage1FlowObjective.DefendCamp;

            if (expeditionActivity == ExpeditionActivityStatus.Active)
                return Stage1FlowObjective.ClearExpedition;

            if (stage < Stage1SettlementProgressStage.CampRepaired)
                return Stage1FlowObjective.RepairCamp;

            if (stage < Stage1SettlementProgressStage.WorkerAssigned)
                return Stage1FlowObjective.AssignWorker;

            if (stage < Stage1SettlementProgressStage.StockpilePlaced)
                return Stage1FlowObjective.PlaceStockpile;

            if (stage < Stage1SettlementProgressStage.ShelterPlaced)
                return Stage1FlowObjective.PlaceShelter;

            if (stage < Stage1SettlementProgressStage.ExtractionOnline)
                return Stage1FlowObjective.BringExtractionOnline;

            if (stage < Stage1SettlementProgressStage.WorkbenchOnline)
                return Stage1FlowObjective.BringWorkbenchOnline;

            if (stage < Stage1SettlementProgressStage.LoadoutPrepared)
                return Stage1FlowObjective.PrepareBuild;

            if (expeditionAvailability == ExpeditionAvailabilityStatus.Available)
                return Stage1FlowObjective.StartExpedition;

            return Stage1FlowObjective.PrepareBuild;
        }

        private static Stage1FlowHint ResolveHint(Stage1SettlementProgressStage stage)
        {
            if (stage == Stage1SettlementProgressStage.RepairResourcesReady)
                return Stage1FlowHint.ContinueRepairBuild;

            if (stage < Stage1SettlementProgressStage.RepairResourcesReady)
                return Stage1FlowHint.GatherRepairResources;

            if (stage == Stage1SettlementProgressStage.CampRepaired)
                return Stage1FlowHint.AssignWorker;

            if (stage == Stage1SettlementProgressStage.WorkerAssigned)
                return Stage1FlowHint.PlaceStockpile;

            if (stage == Stage1SettlementProgressStage.StockpilePlaced)
                return Stage1FlowHint.PlaceShelter;

            if (stage == Stage1SettlementProgressStage.ShelterPlaced)
                return Stage1FlowHint.BringExtractionOnline;

            if (stage == Stage1SettlementProgressStage.ExtractionOnline)
                return Stage1FlowHint.BringWorkbenchOnline;

            return Stage1FlowHint.None;
        }

        public void Dispose()
        {
            if (CW.Status != WorldStatus.NotCreated)
                CW.Destroy();
        }
    }
}
