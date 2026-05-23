using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Combat;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Features.Shared;
using StaticMlp.Features.Effects;
using StaticMlp.Features.Npc;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Statuses;
using StaticMlp.Features.CampFlow;
using StaticMlp.Features.Player;
using StaticMlp.Game;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Replication.Generated;
using StaticMlp.Networking.Transport;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class CombatTestServerWorldScope : IDisposable
    {
        public const float DEFAULT_FIXED_STEP_SECONDS = 1f / 30f;

        public CombatTestServerWorldScope()
        {
            if (SW.Status != WorldStatus.NotCreated)
                SW.Destroy();

            ServerPeerRegistry.Clear();
            NetworkEventRegistry.Clear();
            ReplicatedNetworkEventRegistry.RegisterNetworkEvents();
            ReplicatedComponentRegistration.RegisterReplicationComponents();
            new LoadoutLogicFeature().RegisterNetworkEvents();
            new CombatLogicFeature().RegisterNetworkEvents();
            new BuildingsGameplayFeature().RegisterNetworkEvents();
            new FrontierLogicFeature().RegisterNetworkEvents();
            new CampFlowGameplayFeature().RegisterNetworkEvents();
            SW.Create(WorldConfig.Default());
            SW.Types().RegisterAll(
                typeof(NpcTag).Assembly,
                typeof(ServerWT).Assembly,
                typeof(LoadoutLogicFeature).Assembly,
                typeof(BuildingConstructionCompletedEvent).Assembly,
                typeof(BuildingsGameplayFeature).Assembly,
                typeof(CombatLogicFeature).Assembly,
                typeof(EffectProcessedTag).Assembly,
                typeof(EffectsLogicFeature).Assembly,
                typeof(FrontierLogicFeature).Assembly,
                typeof(Health).Assembly,
                typeof(ProgressFlagAppliedEvent).Assembly,
                typeof(ProgressionLogicFeature).Assembly,
                typeof(SettlementAnchorRef).Assembly,
                typeof(SettlementSharedResourcesGameplayFeature).Assembly,
                typeof(SettlementWorkersGameplayFeature).Assembly,
                typeof(CampFlowViewState).Assembly,
                typeof(CampFlowGameplayFeature).Assembly,
                typeof(PoisonStatus).Assembly,
                typeof(StatusesLogicFeature).Assembly,
                typeof(CharacterNetState).Assembly,
                typeof(AiAgentTag).Assembly);
            NetworkEventRegistry.RegisterServerWorldTypes();
            SW.Initialize();
            SW.SetResource(new GameTime());
            SW.SetResource(new SimulationTime
            {
                FixedStepSeconds = DEFAULT_FIXED_STEP_SECONDS,
            });
            SW.SetResource(new CombatDebugLogBuffer());
            SW.SetResource(new NetOutbox());
            SW.SetResource(new CombatConfig());
            SW.SetResource<IHeightSampler>(new FlatTestHeightSampler());
            SW.SetResource(FrontierSeedManifest.CreateResource());
            SW.SetResource(ProgressionSeedManifest.CreateResource());
            SW.SetResource(new StatusesConfig());
            SW.SetResource(new AiBotFactory());
            SW.SetResource(new BuildingEntityFactory());
            SW.SetResource(new PlayerFactory());
            SW.SetResource(new SettlementSharedResourcesFactory());
            SW.SetResource(new SettlementWorkerFactory());
            SW.SetResource(new CampFlowAnchorFactory());
            SW.SetResource(new StatusEntityFactory());
        }

        public CombatDebugLogBuffer DebugLog => SW.GetResource<CombatDebugLogBuffer>();
        public GameTime Time => SW.GetResource<GameTime>();
        public SimulationTime SimulationTime => SW.GetResource<SimulationTime>();

        public void SetGameTime(float time, float deltaTime = 0f)
        {
            var gameTime = Time;
            gameTime.Time = time;
            gameTime.DeltaTime = deltaTime;
        }

        public void SetSimulationTime(uint serverTick, float fixedStepSeconds = DEFAULT_FIXED_STEP_SECONDS, double? elapsedSeconds = null)
        {
            var simulationTime = SimulationTime;
            simulationTime.ServerTick = serverTick;
            simulationTime.FixedStepSeconds = fixedStepSeconds;
            simulationTime.ElapsedSeconds = elapsedSeconds ?? serverTick * fixedStepSeconds;
        }

        public void AdvanceSimulationTicks(uint ticks)
        {
            var simulationTime = SimulationTime;
            simulationTime.ServerTick += ticks;
            simulationTime.ElapsedSeconds += ticks * simulationTime.FixedStepSeconds;
        }

        public void AdvanceSimulationSeconds(float seconds)
        {
            AdvanceSimulationTicks(SecondsToTicks(seconds));
        }

        public uint SecondsToTicks(float seconds)
        {
            return SimulationTime.SecondsToTicks(seconds);
        }

        public SW.Entity CreateEntity()
        {
            return SW.NewEntity<Default>();
        }

        public SW.Entity CreateEntityWithHealth(float current = 100f, float max = 100f)
        {
            var entity = SW.NewEntity<Default>();
            entity.Set(new Health
            {
                Current = current,
                Max = max,
            });
            return entity;
        }

        public SW.Entity CreatePlayer(NetworkPeerId owner, Vector3 position)
        {
            var entity = SW.NewEntity<Default>();
            entity.Set<PlayerTag>();
            entity.Set(new NetworkIdentity
            {
                Owner = owner,
                Authority = NetworkAuthority.Owner,
                NetworkArchetypeId = 0
            });
            entity.Set(new CharacterNetState
            {
                Position = position,
                Rotation = Quaternion.identity
            });
            entity.Set(new ServerCombatAttackState());
            entity.Set(new Health
            {
                Current = 100f,
                Max = 100f
            });
            entity.Set(LoadoutPreparationRules.DefaultSelection());
            entity.Set(LoadoutPreparationRules.CreatePreparedSnapshot(entity.Read<OwnerLoadoutSelection>()));
            return entity;
        }

        public SW.Entity CreateMonsterWithHealth(Vector3 position, float current = 100f, float max = 100f)
        {
            var entity = SW.NewEntity<Default>();
            entity.Set<MonsterTag>();
            entity.Set(new NetworkIdentity
            {
                Owner = new NetworkPeerId(1),
                Authority = NetworkAuthority.Server,
                NetworkArchetypeId = 0
            });
            entity.Set(new CharacterNetState
            {
                Position = position,
                Rotation = Quaternion.identity
            });
            entity.Set(new ServerCombatAttackState());
            entity.Set(new Health
            {
                Current = current,
                Max = max
            });
            return entity;
        }

        public SW.Entity CreateSettlementAnchor(
            SettlementAnchorId anchorId,
            Vector3 position,
            CampFlowStage stage = CampFlowStage.LoadoutPrepared)
        {
            var entity = SW.NewEntity<Default>();
            entity.Set(new CampFlowProgression
            {
                AnchorId = anchorId.Value,
                Stage = stage
            });
            entity.Set(new CampFlowViewState
            {
                AnchorId = anchorId.Value,
                Stage = stage,
                Objective = stage < CampFlowStage.CampRepaired
                    ? Stage1FlowObjective.RepairCamp
                    : stage < CampFlowStage.WorkerAssigned
                        ? Stage1FlowObjective.AssignWorker
                        : stage < CampFlowStage.StockpilePlaced
                            ? Stage1FlowObjective.PlaceStockpile
                            : stage < CampFlowStage.ShelterPlaced
                                ? Stage1FlowObjective.PlaceShelter
                                : stage < CampFlowStage.ExtractionOnline
                                    ? Stage1FlowObjective.BringExtractionOnline
                                    : stage < CampFlowStage.WorkbenchOnline
                                        ? Stage1FlowObjective.BringWorkbenchOnline
                                        : Stage1FlowObjective.PrepareBuild,
                Hint = stage == CampFlowStage.RepairResourcesReady
                    ? Stage1FlowHint.ContinueRepairBuild
                    : stage < CampFlowStage.RepairResourcesReady
                        ? Stage1FlowHint.GatherRepairResources
                        : stage == CampFlowStage.CampRepaired
                            ? Stage1FlowHint.AssignWorker
                            : stage == CampFlowStage.WorkerAssigned
                                ? Stage1FlowHint.PlaceStockpile
                                : stage == CampFlowStage.StockpilePlaced
                                    ? Stage1FlowHint.PlaceShelter
                                    : stage == CampFlowStage.ShelterPlaced
                                        ? Stage1FlowHint.BringExtractionOnline
                                        : stage == CampFlowStage.ExtractionOnline
                                            ? Stage1FlowHint.BringWorkbenchOnline
                                            : Stage1FlowHint.None,
                CanToggleWorkerAssignment = stage >= CampFlowStage.CampRepaired,
                CanOpenLoadoutPreparation = stage >= CampFlowStage.WorkbenchOnline
            });
            entity.Set(new ProgressionState(anchorId, 0u));
            entity.Set(new SettlementAnchorLocation(position, Quaternion.identity));
            entity.Set(new SettlementWorkerSummary
            {
                AnchorId = anchorId.Value
            });
            entity.Set(new SettlementCampBuilderJobState
            {
                AnchorId = anchorId.Value
            });
            entity.Set(new ExpeditionAvailabilityState
            {
                ExpeditionIdValue = ExpeditionCatalog.NearbyRaiderCampId.Value,
                Status = ExpeditionAvailabilityStatus.Unavailable
            });
            entity.Set(new ActiveExpeditionState
            {
                Status = ExpeditionActivityStatus.None
            });
            entity.Set(new ThreatState
            {
                Phase = ThreatPhase.Calm
            });
            entity.Set(new RaidScheduleState
            {
                Status = RaidScheduleStatus.None
            });
            entity.Set(new BossEncounterState
            {
                BossIdValue = BossCatalog.RaiderChiefId.Value,
                Status = BossEncounterStatus.Unavailable
            });
            entity.Set(new BossLoadoutPreparationState
            {
                Status = BossLoadoutPreparationStatus.None
            });
            entity.Set(new BossPreparedLoadoutSnapshot());
            return entity;
        }

        public SW.Entity CreateSettlementSharedResources(int wood = 50, int stone = 25)
        {
            var entity = SW.NewEntity<Default>();
            entity.Set<SettlementResourceStorageTag>();
            entity.Set(new SettlementSharedResources { Capacity = int.MaxValue });
            ref var rows = ref entity.Add<SW.Multi<SettlementStoredResource>>();
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

        public SW.Entity CreateNetworkedConstructionSite(
            BuildingId? buildingId = null,
            ConstructionPhase phase = ConstructionPhase.ReadyToBuild,
            float buildWorkRequired = 100f,
            float buildWorkDone = 0f,
            int woodRequired = 10,
            int stoneRequired = 4,
            Vector3? position = null,
            NetworkPeerId? owner = null)
        {
            var definition = BuildingCatalogData.Get(buildingId ?? BuildingCatalogData.CampCoreId);
            var gid = SW.GetResource<BuildingEntityFactory>().SpawnConstructionSite(new ConstructionSiteSpawnSpec(
                owner ?? new NetworkPeerId(1),
                definition,
                SettlementAnchorCatalog.HomeCampId,
                position ?? Vector3.zero,
                Quaternion.identity,
                startReadyToBuild: true,
                initialBuildWork: 0f));

            if (!gid.TryUnpack<ServerWT>(out var entity))
                throw new InvalidOperationException("Spawned construction site could not be unpacked in server world.");

            ref var state = ref entity.Mut<ConstructionSiteState>();
            state.Phase = phase;

            ref var resources = ref entity.Ref<SW.Multi<ConstructionResourceEntry>>();
            resources.Clear();
            resources.Add(new ConstructionResourceEntry(ResourceCatalog.WoodId, woodRequired, woodRequired));
            resources.Add(new ConstructionResourceEntry(ResourceCatalog.StoneId, stoneRequired, stoneRequired));

            ref var progress = ref entity.Mut<ConstructionProgress>();
            progress.BuildWorkRequired = buildWorkRequired;
            progress.BuildWorkDone = buildWorkDone;

            return entity;
        }

        public void Dispose()
        {
            ServerPeerRegistry.Clear();
            if (SW.Status != WorldStatus.NotCreated)
                SW.Destroy();
        }

        private sealed class FlatTestHeightSampler : IHeightSampler
        {
            public float SampleHeight(float worldX, float worldZ) => 0f;
        }
    }
}
