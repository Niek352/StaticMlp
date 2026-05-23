using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Player;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Features.CampFlow;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Replication.Generated;
using StaticMlp.Networking.Requests;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class CampFlowPresentationClientWorldScope : IDisposable
    {
        public CampFlowPresentationClientWorldScope()
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
            new CampFlowGameplayFeature().RegisterNetworkEvents();


            CW.Create(WorldConfig.Default());
            CW.Types().RegisterAll(
                typeof(ClientCoreWT).Assembly,
                typeof(SettlementAnchorRef).Assembly,
                typeof(SettlementSharedResourcesGameplayFeature).Assembly,
                typeof(SettlementPresentationFeature).Assembly,
                typeof(BuildingWorkerAssignmentState).Assembly,
                typeof(SettlementWorkersGameplayFeature).Assembly,
                typeof(LoadoutLogicFeature).Assembly,
                typeof(LoadoutPresentationFeature).Assembly,
                typeof(BuildingsGameplayFeature).Assembly,
                typeof(FrontierLogicFeature).Assembly,
                typeof(FrontierPresentationFeature).Assembly,
                typeof(ProgressionLogicFeature).Assembly,
                typeof(ProgressionPresentationFeature).Assembly,
                typeof(CampFlowViewState).Assembly,
                typeof(CampFlowGameplayFeature).Assembly,
                typeof(PlayerTag).Assembly);
            ProjectionRegistry.RegisterClientWorldTypes();
            NetworkEventRegistry.RegisterClientWorldTypes();
            CW.Initialize();
            CW.SetResource(new NetOutbox());
        }

        public CW.Entity CreateAnchor(
            CampFlowStage stage = CampFlowStage.DamagedCampStart,
            ThreatPhase threatPhase = ThreatPhase.Calm,
            RaidScheduleStatus raidStatus = RaidScheduleStatus.None,
            ExpeditionAvailabilityStatus expeditionAvailability = ExpeditionAvailabilityStatus.Unavailable,
            ExpeditionActivityStatus expeditionActivity = ExpeditionActivityStatus.None,
            BossEncounterStatus bossStatus = BossEncounterStatus.Unavailable)
        {
            var anchor = CW.NewEntity<Default>();
            anchor.Set(new CampFlowProgression
            {
                AnchorId = SettlementAnchorCatalog.HomeCampId.Value,
                Stage = stage
            });
            anchor.Set(new ProgressionState(SettlementAnchorCatalog.HomeCampId, 0));
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
            player.Set(LoadoutPreparationRules.CreatePreparedSnapshot(player.Read<OwnerLoadoutSelection>()));
            player.Set(new ClientLoadoutSelectionSyncState());
            return player;
        }

        public CW.Entity CreateConstructionSite(ConstructionPhase phase, int woodRequired, int woodDelivered, int stoneRequired, int stoneDelivered, float progress01)
        {
            return CreateConstructionSite(
                BuildingCatalogData.CampCoreId,
                phase,
                woodRequired,
                woodDelivered,
                stoneRequired,
                stoneDelivered,
                progress01,
                new Vector3(0f, 0f, 1f));
        }

        public CW.Entity CreateConstructionSite(
            ConstructionPhase phase,
            int woodRequired,
            int woodDelivered,
            int stoneRequired,
            int stoneDelivered,
            float progress01,
            Vector3 position)
        {
            return CreateConstructionSite(
                BuildingCatalogData.CampCoreId,
                phase,
                woodRequired,
                woodDelivered,
                stoneRequired,
                stoneDelivered,
                progress01,
                position);
        }

        public CW.Entity CreateConstructionSite(
            BuildingId buildingId,
            ConstructionPhase phase,
            int woodRequired,
            int woodDelivered,
            int stoneRequired,
            int stoneDelivered,
            float progress01,
            Vector3 position)
        {
            var site = CW.NewEntity<Default>();
            site.Set<ConstructionSiteTag>();
            site.Set(new ConstructionTransform
            {
                Position = position,
                Rotation = Quaternion.identity
            });
            site.Set(new ConstructionSiteState
            {
                BuildingId = buildingId.Value,
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

            foreach (var anchor in CW.Query<All<CampFlowProgression>>().Entities())
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

        private static CampFlowViewState CreateFlowViewState(
            CampFlowStage stage,
            ExpeditionAvailabilityStatus expeditionAvailability,
            ExpeditionActivityStatus expeditionActivity,
            ThreatPhase threatPhase,
            RaidScheduleStatus raidStatus,
            BossEncounterStatus bossStatus)
        {
            return new CampFlowViewState
            {
                AnchorId = SettlementAnchorCatalog.HomeCampId.Value,
                Stage = stage,
                Objective = ResolveObjective(stage, expeditionAvailability, expeditionActivity, threatPhase, bossStatus),
                Hint = ResolveHint(stage),
                CanToggleWorkerAssignment = stage >= CampFlowStage.CampRepaired,
                CanOpenLoadoutPreparation = stage >= CampFlowStage.WorkbenchOnline
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
            CampFlowStage stage,
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

            if (stage < CampFlowStage.CampRepaired)
                return Stage1FlowObjective.RepairCamp;

            if (stage < CampFlowStage.WorkerAssigned)
                return Stage1FlowObjective.AssignWorker;

            if (stage < CampFlowStage.StockpilePlaced)
                return Stage1FlowObjective.PlaceStockpile;

            if (stage < CampFlowStage.ShelterPlaced)
                return Stage1FlowObjective.PlaceShelter;

            if (stage < CampFlowStage.ExtractionOnline)
                return Stage1FlowObjective.BringExtractionOnline;

            if (stage < CampFlowStage.WorkbenchOnline)
                return Stage1FlowObjective.BringWorkbenchOnline;

            if (stage < CampFlowStage.LoadoutPrepared)
                return Stage1FlowObjective.PrepareBuild;

            if (expeditionAvailability == ExpeditionAvailabilityStatus.Available)
                return Stage1FlowObjective.StartExpedition;

            return Stage1FlowObjective.PrepareBuild;
        }

        private static Stage1FlowHint ResolveHint(CampFlowStage stage)
        {
            if (stage == CampFlowStage.RepairResourcesReady)
                return Stage1FlowHint.ContinueRepairBuild;

            if (stage < CampFlowStage.RepairResourcesReady)
                return Stage1FlowHint.GatherRepairResources;

            if (stage == CampFlowStage.CampRepaired)
                return Stage1FlowHint.AssignWorker;

            if (stage == CampFlowStage.WorkerAssigned)
                return Stage1FlowHint.PlaceStockpile;

            if (stage == CampFlowStage.StockpilePlaced)
                return Stage1FlowHint.PlaceShelter;

            if (stage == CampFlowStage.ShelterPlaced)
                return Stage1FlowHint.BringExtractionOnline;

            if (stage == CampFlowStage.ExtractionOnline)
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
