using System;
using FFS.Libraries.StaticEcs;
using FFS.Libraries.StaticPack;
using NUnit.Framework;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Features.Stage1;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
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
            var completedReceiver = SW.RegisterEventReceiver<BuildingConstructionCompletedEvent>();

            ref var siteState = ref site.Mut<ConstructionSiteState>();
            ref var siteProgress = ref site.Mut<ConstructionProgress>();
            var applied = SettlementConstructionRules.ApplyBuildWork(site, ref siteState, ref siteProgress, 10f, 10f);
            Assert.That(applied, Is.True);
            Assert.That(siteState.Phase, Is.EqualTo(ConstructionPhase.Completed));

            new ServerCompleteConstructionSystem().Update();
            flowSystem.Update();
            flowSystem.Destroy();

            Assert.That(siteGid.TryUnpack<ServerWT>(out _), Is.False);
            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.CampRepaired));

            var finishedCount = 0;
            EntityGID finishedGid = default;
            foreach (var finished in SW.Query<All<FinishedBuildingTag, SettlementAnchorRef, ConstructionSiteState, ConstructionProgress>>().Entities())
            {
                finishedCount++;
                finishedGid = finished.GID;
                Assert.That(finished.Read<SettlementAnchorRef>().AnchorId, Is.EqualTo(SettlementAnchorCatalog.HomeCampId.Value));
                Assert.That(finished.Read<ConstructionSiteState>().Phase, Is.EqualTo(ConstructionPhase.Completed));
                Assert.That(finished.Read<ConstructionProgress>().IsComplete, Is.True);
            }

            Assert.That(finishedCount, Is.EqualTo(1));

            var completedCount = 0;
            foreach (var evt in completedReceiver)
            {
                completedCount++;
                Assert.That(evt.Value.FinishedBuilding, Is.EqualTo(finishedGid));
                Assert.That(evt.Value.BuildingId, Is.EqualTo(BuildingCatalogData.CampCoreId));
                Assert.That(evt.Value.AnchorId, Is.EqualTo(SettlementAnchorCatalog.HomeCampId));
                Assert.That(evt.Value.Position, Is.EqualTo(Vector3.zero));
            }

            SW.DeleteEventReceiver(ref completedReceiver);
            Assert.That(completedCount, Is.EqualTo(1));
        }

        [Test]
        public void ServerCompleteConstructionSystem_WhenNonCampCoreFinishes_EmitsGenericFactWithoutRepairFact()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Stage1SettlementProgressStage.RepairResourcesReady);
            var site = scope.CreateNetworkedConstructionSite(BuildingCatalogData.StockpileId, buildWorkDone: 100f);
            ref var siteState = ref site.Mut<ConstructionSiteState>();
            siteState.Phase = ConstructionPhase.Completed;

            var flowSystem = new ServerStage1FlowSystem();
            flowSystem.Init();
            var completedReceiver = SW.RegisterEventReceiver<BuildingConstructionCompletedEvent>();

            new ServerCompleteConstructionSystem().Update();
            flowSystem.Update();
            flowSystem.Destroy();

            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.RepairResourcesReady));

            var completedCount = 0;
            foreach (var evt in completedReceiver)
            {
                completedCount++;
                Assert.That(evt.Value.BuildingId, Is.EqualTo(BuildingCatalogData.StockpileId));
                Assert.That(evt.Value.AnchorId, Is.EqualTo(SettlementAnchorCatalog.HomeCampId));
                Assert.That(evt.Value.FinishedBuilding.TryUnpack<ServerWT>(out var finished), Is.True);
                Assert.That(finished.Read<ConstructionSiteState>().BuildingId, Is.EqualTo(BuildingCatalogData.StockpileId.Value));
            }

            SW.DeleteEventReceiver(ref completedReceiver);
            Assert.That(completedCount, Is.EqualTo(1));
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
            new ServerInitialConstructionSiteSpawnSystem().Update();

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
            Assert.That(anchor.Has<BossLoadoutPreparationState>(), Is.True);
            Assert.That(anchor.Has<BossPreparedLoadoutSnapshot>(), Is.True);
            Assert.That(anchor.Has<SettlementAnchorLocation>(), Is.True);

            var siteCount = 0;
            foreach (var site in SW.Query<All<ConstructionSiteTag, ConstructionSiteState, SettlementAnchorRef>>().Entities())
            {
                siteCount++;
                Assert.That(site.Read<SettlementAnchorRef>().AnchorId, Is.EqualTo(SettlementAnchorCatalog.HomeCampId.Value));
                Assert.That(site.Read<ConstructionSiteState>().BuildingId, Is.EqualTo(BuildingCatalogData.CampCoreId.Value));
            }

            Assert.That(siteCount, Is.EqualTo(1));
        }

        [Test]
        public void ServerStage1SeedSpawnSystems_ProjectInitialCampAndWorkerToTerrainHeight()
        {
            using var scope = new CombatTestServerWorldScope();
            SW.SetResource<IHeightSampler>(new ConstantHeightSampler(7.25f));
            SW.SetResource(Stage1SettlementSeedManifest.CreateResource());

            new ServerStage1CampAnchorSpawnSystem().Update();
            new ServerInitialConstructionSiteSpawnSystem().Update();
            new ServerSettlementWorkerSpawnSystem().Update();

            var anchor = Stage1SettlementProgressionQuery.GetServerAnchor(SettlementAnchorCatalog.HomeCampId);
            Assert.That(anchor.Read<SettlementAnchorLocation>().Position.y, Is.EqualTo(7.25f));

            var siteCount = 0;
            foreach (var site in SW.Query<All<ConstructionSiteTag, ConstructionTransform>>().Entities())
            {
                siteCount++;
                Assert.That(site.Read<ConstructionTransform>().Position.y, Is.EqualTo(7.25f));
            }

            Assert.That(siteCount, Is.EqualTo(1));

            var workerCount = 0;
            foreach (var worker in SW.Query<All<SettlementWorkerTag, CharacterNetState>>().Entities())
            {
                workerCount++;
                Assert.That(worker.Read<CharacterNetState>().Position.y, Is.EqualTo(7.25f));
            }

            Assert.That(workerCount, Is.EqualTo(1));
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
        public void ServerStage1FlowSystem_WhenLoadoutPreparedEventReceived_AdvancesToLoadoutPrepared()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Stage1SettlementProgressStage.WorkbenchOnline);

            var flowSystem = new ServerStage1FlowSystem();
            flowSystem.Init();
            SW.SendEvent(new Stage1LoadoutPreparedEvent(SettlementAnchorCatalog.HomeCampId));
            flowSystem.Update();
            flowSystem.Destroy();

            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.LoadoutPrepared));
        }

        [Test]
        public void ServerFrontierExpeditionAvailabilitySystem_WhenPreparedLoadoutSnapshotExists_DoesNotAdvanceStage()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Stage1SettlementProgressStage.WorkerAssigned);
            scope.CreatePlayer(new NetworkPeerId(1), Vector3.zero);

            new ServerPreparedLoadoutSnapshotSystem().Update();
            new ServerFrontierExpeditionAvailabilitySystem().Update();

            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.WorkerAssigned));
        }

        [Test]
        public void ClientStage1HudStateSystem_WhenCampIsRepaired_ShowsAssignWorkerAndKeepsLoadoutPreparationClosed()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            scope.CreateAnchor(stage: Stage1SettlementProgressStage.CampRepaired);
            scope.CreateSharedResources();
            scope.RefreshProjections();

            new ClientStage1HudStateSystem().Update();

            ref readonly var hud = ref CW.GetResource<Stage1HudState>();
            Assert.That(hud.Objective, Is.Not.EqualTo(Stage1ObjectiveKind.RepairCamp));
            Assert.That(hud.Objective, Is.EqualTo(Stage1ObjectiveKind.AssignWorker));
            Assert.That(hud.CanOpenLoadoutPreparation, Is.False);
            Assert.That(hud.CanOpenExpeditionSelection, Is.False);
        }

        [Test]
        public void Stage1RepairFlow_OneBuildActionDoesNotCompleteInitialCampRepair()
        {
            using var scope = new CombatTestServerWorldScope();
            var site = scope.CreateNetworkedConstructionSite(phase: ConstructionPhase.ReadyToBuild);
            ref var state = ref site.Mut<ConstructionSiteState>();
            ref var progress = ref site.Mut<ConstructionProgress>();
            progress = new ConstructionProgress
            {
                BuildWorkRequired = 100f,
                BuildWorkDone = 0f
            };

            var applied = SettlementConstructionRules.ApplyBuildWork(site, ref state, ref progress, 35f / 30f, 5f);

            Assert.That(applied, Is.True);
            Assert.That(state.Phase, Is.EqualTo(ConstructionPhase.BuildingInProgress));
            Assert.That(progress.BuildWorkDone, Is.LessThan(progress.BuildWorkRequired));
            Assert.That(progress.Normalized, Is.LessThan(1f));
        }

        [Test]
        public void SettlementConstructionRules_WhenPlanksAreRequired_DepositsNonWoodStoneResource()
        {
            var state = new ConstructionSiteState
            {
                Phase = ConstructionPhase.WaitingForResources
            };
            using var scope = new CombatTestServerWorldScope();
            var site = scope.CreateEntity();
            site.Set(new ConstructionResources());
            ConstructionResourcesAccess.InitializeRows(site, new[]
            {
                new ResourceAmount(ResourceCatalog.PlanksId, 3)
            });
            var storage = scope.CreateSettlementSharedResources(wood: 0, stone: 0);
            SettlementSharedResourcesAccess.Add(storage, ResourceCatalog.PlanksId, 5);

            var planned = SettlementConstructionRules.TryPlanResourceDeposit(
                in state,
                site,
                storage,
                new[] { new ResourceAmount(ResourceCatalog.PlanksId, 5) },
                out var acceptedResources);

            Assert.That(planned, Is.True);
            Assert.That(acceptedResources.Length, Is.EqualTo(1));
            Assert.That(acceptedResources[0].Id, Is.EqualTo(ResourceCatalog.PlanksId));
            Assert.That(acceptedResources[0].Amount, Is.EqualTo(3));

            var applied = SettlementConstructionRules.ApplyResourceDeposit(
                site,
                ref state,
                acceptedResources);

            Assert.That(applied, Is.True);
            Assert.That(ConstructionResourcesAccess.GetDelivered(site, ResourceCatalog.PlanksId), Is.EqualTo(3));
            Assert.That(ConstructionResourcesAccess.IsComplete(site), Is.True);
            Assert.That(state.Phase, Is.EqualTo(ConstructionPhase.ReadyToBuild));
        }

        [Test]
        public void SettlementSharedResourcesFactory_SpawnCreatesCatalogStorageRows()
        {
            using var scope = new CombatTestServerWorldScope();
            var seed = Stage1SettlementSeedManifest.CreateResource();
            var gid = SW.GetResource<SettlementSharedResourcesFactory>().Spawn(seed);

            Assert.That(gid.TryUnpack<ServerWT>(out var storage), Is.True);
            ref readonly var rows = ref storage.Ref<SW.Multi<SettlementStoredResource>>();
            var expectedCount = 0;

            for (var i = 0; i < ResourceCatalog.All.Count; i++)
            {
                var definition = ResourceCatalog.All[i];
                if (!definition.IsSettlementStored)
                    continue;

                expectedCount++;
                Assert.That(SettlementSharedResourcesAccess.GetAmount(storage, definition.Id), Is.EqualTo(seed.GetStartingResourceAmount(definition.Id)));
            }

            Assert.That(rows.Length, Is.EqualTo(expectedCount));
        }

        [Test]
        public void SettlementSharedResourcesAccess_AddSpendSupportsArbitraryCatalogResourcesAndCapacity()
        {
            using var scope = new CombatTestServerWorldScope();
            var storage = scope.CreateSettlementSharedResources(wood: 0, stone: 0);
            ref var host = ref ReplicationMut.Mut<SettlementSharedResources>(storage);
            host.Capacity = 5;

            Assert.That(SettlementSharedResourcesAccess.Add(storage, ResourceCatalog.PlanksId, 8), Is.EqualTo(5));
            Assert.That(SettlementSharedResourcesAccess.GetAmount(storage, ResourceCatalog.PlanksId), Is.EqualTo(5));
            Assert.That(SettlementSharedResourcesAccess.Add(storage, ResourceCatalog.WoodId, 1), Is.EqualTo(0));
            Assert.That(SettlementSharedResourcesAccess.Spend(storage, ResourceCatalog.PlanksId, 3), Is.EqualTo(3));
            Assert.That(SettlementSharedResourcesAccess.GetAmount(storage, ResourceCatalog.PlanksId), Is.EqualTo(2));
            Assert.Throws<InvalidOperationException>(() => SettlementSharedResourcesAccess.Add(storage, new ResourceId(ushort.MaxValue), 1));
        }

        [Test]
        public void SettlementSharedResourcesCodec_CopiesCapacityAndRowsServerToClient()
        {
            using var serverScope = new CombatTestServerWorldScope();
            using var clientScope = new Stage1PresentationClientWorldScope();
            var storage = serverScope.CreateSettlementSharedResources(wood: 12, stone: 6);
            ref var serverHost = ref ReplicationMut.Mut<SettlementSharedResources>(storage);
            serverHost.Capacity = 99;
            SettlementSharedResourcesAccess.Add(storage, ResourceCatalog.PlanksId, 4);

            var writer = BinaryPackWriter.CreateFromPool();
            serverHost.Write(ref writer, storage);
            var bytes = writer.CopyToBytes();
            writer.Dispose();

            var clientStorage = CW.NewEntity<Default>();
            clientStorage.Set(new SettlementSharedResources());
            var reader = new BinaryPackReader(bytes, (uint)bytes.Length, 0);
            ref var clientHost = ref clientStorage.Mut<SettlementSharedResources>();
            clientHost.Read(ref reader, clientStorage, version: 1, disabled: false);

            Assert.That(clientHost.Capacity, Is.EqualTo(99));
            Assert.That(SettlementSharedResourcesAccess.GetAmount(clientStorage, ResourceCatalog.WoodId), Is.EqualTo(12));
            Assert.That(SettlementSharedResourcesAccess.GetAmount(clientStorage, ResourceCatalog.StoneId), Is.EqualTo(6));
            Assert.That(SettlementSharedResourcesAccess.GetAmount(clientStorage, ResourceCatalog.PlanksId), Is.EqualTo(4));
            Assert.That(clientStorage.Ref<CW.Multi<SettlementStoredResource>>().Length, Is.EqualTo(storage.Ref<SW.Multi<SettlementStoredResource>>().Length));
        }

        [Test]
        public void DepositConstructionResourcesProjector_MutatesProjectedMultiRowsOnly()
        {
            using var scope = new Stage1PresentationClientWorldScope();
            var storage = scope.CreateSharedResources(wood: 50, stone: 25);
            var site = scope.CreateConstructionSite(
                ConstructionPhase.WaitingForResources,
                woodRequired: 10,
                woodDelivered: 0,
                stoneRequired: 0,
                stoneDelivered: 0,
                progress01: 0f);
            scope.RefreshProjections();

            new DepositConstructionResourcesProjector().Project(new DepositConstructionResourcesRequestEvent(
                site.GID,
                new[] { new ResourceAmount(ResourceCatalog.WoodId, 10) }));

            Assert.That(SettlementSharedResourcesAccess.GetAmount(storage, ResourceCatalog.WoodId), Is.EqualTo(50));
            Assert.That(ConstructionResourcesAccess.GetDelivered(site, ResourceCatalog.WoodId), Is.EqualTo(0));
            Assert.That(storage.Read<SettlementSharedResources>().Capacity, Is.EqualTo(ClientProjection.Read<SettlementSharedResources>(storage).Capacity));
            Assert.That(SettlementSharedResourcesAccess.GetProjectedAmount(storage, ResourceCatalog.WoodId), Is.EqualTo(40));
            Assert.That(ConstructionResourcesAccess.GetProjectedDelivered(site, ResourceCatalog.WoodId), Is.EqualTo(10));
            Assert.That(site.Read<ConstructionSiteState>().Phase, Is.EqualTo(ConstructionPhase.WaitingForResources));
            Assert.That(ClientProjection.Read<ConstructionSiteState>(site).Phase, Is.EqualTo(ConstructionPhase.ReadyToBuild));
        }

        [Test]
        public void DepositConstructionResourcesRequestCodec_RoundTripsResourceArray()
        {
            using var serverScope = new CombatTestServerWorldScope();
            using var clientScope = new Stage1PresentationClientWorldScope();
            SW.SetResource(new NetInbox());
            CW.GetResource<NetOutbox>().Clear();
            var receiver = SW.RegisterEventReceiver<NetworkEventFromClient<DepositConstructionResourcesRequestEvent>>();
            var request = new DepositConstructionResourcesRequestEvent(
                default,
                new[]
                {
                    new ResourceAmount(ResourceCatalog.WoodId, 3),
                    new ResourceAmount(ResourceCatalog.PlanksId, 2)
                })
            {
                RequestId = new RequestId(123)
            };

            CW.SendToServer(request);
            CW.GetResource<NetOutbox>().FlushNetworkEventBatches();
            var packet = CW.GetResource<NetOutbox>().Packets[0];

            Assert.That(PacketCodec.Decode(new NetworkPeerId(1), packet.Payload, SW.GetResource<NetInbox>()), Is.True);
            new ServerNetworkEventApplySystem().Update();

            var receivedCount = 0;
            foreach (var received in receiver)
            {
                receivedCount++;
                Assert.That(received.Value.SourcePeer, Is.EqualTo(new NetworkPeerId(1)));
                Assert.That(received.Value.Value.RequestId, Is.EqualTo(new RequestId(123)));
                Assert.That(received.Value.Value.Resources.Length, Is.EqualTo(2));
                Assert.That(received.Value.Value.Resources[0].Id, Is.EqualTo(ResourceCatalog.WoodId));
                Assert.That(received.Value.Value.Resources[0].Amount, Is.EqualTo(3));
                Assert.That(received.Value.Value.Resources[1].Id, Is.EqualTo(ResourceCatalog.PlanksId));
                Assert.That(received.Value.Value.Resources[1].Amount, Is.EqualTo(2));
            }

            SW.DeleteEventReceiver(ref receiver);
            Assert.That(receivedCount, Is.EqualTo(1));
        }

        [Test]
        public void DepositConstructionResourcesResultCodec_RoundTripsAcceptedResourceArray()
        {
            using var serverScope = new CombatTestServerWorldScope();
            using var clientScope = new Stage1PresentationClientWorldScope();
            CW.SetResource(new NetInbox());
            SW.GetResource<NetOutbox>().Clear();
            var receiver = CW.RegisterEventReceiver<NetworkEventFromServer<DepositConstructionResourcesResultEvent>>();
            var result = new DepositConstructionResourcesResultEvent
            {
                RequestId = new RequestId(456),
                Status = RequestStatus.Accepted,
                Site = default,
                AcceptedResources = new[]
                {
                    new ResourceAmount(ResourceCatalog.StoneId, 4),
                    new ResourceAmount(ResourceCatalog.SimplePartsId, 1)
                }
            };

            SW.SendToPeer(new NetworkPeerId(1), result);
            SW.GetResource<NetOutbox>().FlushNetworkEventBatches();
            var packet = SW.GetResource<NetOutbox>().Packets[0];

            Assert.That(PacketCodec.Decode(NetworkEvents.ServerPeer, packet.Payload, CW.GetResource<NetInbox>()), Is.True);
            new ClientNetworkEventApplySystem().Update();

            var receivedCount = 0;
            foreach (var received in receiver)
            {
                receivedCount++;
                Assert.That(received.Value.SourcePeer, Is.EqualTo(NetworkEvents.ServerPeer));
                Assert.That(received.Value.Value.RequestId, Is.EqualTo(new RequestId(456)));
                Assert.That(received.Value.Value.Status, Is.EqualTo(RequestStatus.Accepted));
                Assert.That(received.Value.Value.AcceptedResources.Length, Is.EqualTo(2));
                Assert.That(received.Value.Value.AcceptedResources[0].Id, Is.EqualTo(ResourceCatalog.StoneId));
                Assert.That(received.Value.Value.AcceptedResources[0].Amount, Is.EqualTo(4));
                Assert.That(received.Value.Value.AcceptedResources[1].Id, Is.EqualTo(ResourceCatalog.SimplePartsId));
                Assert.That(received.Value.Value.AcceptedResources[1].Amount, Is.EqualTo(1));
            }

            CW.DeleteEventReceiver(ref receiver);
            Assert.That(receivedCount, Is.EqualTo(1));
        }

        [Test]
        public void DepositConstructionResourcesHandler_WhenRequestHasNegativeAmount_Rejects()
        {
            using var scope = new CombatTestServerWorldScope();
            var peer = new NetworkPeerId(1);
            scope.CreatePlayer(peer, Vector3.zero);
            var storage = scope.CreateSettlementSharedResources(wood: 50, stone: 25);
            var site = scope.CreateNetworkedConstructionSite(
                phase: ConstructionPhase.WaitingForResources,
                woodRequired: 10,
                stoneRequired: 4);
            ref var rows = ref site.Ref<SW.Multi<ConstructionResourceEntry>>();
            for (var i = 0; i < rows.Length; i++)
            {
                ref var row = ref rows[i];
                row.Delivered = 0;
            }

            var result = new DepositConstructionResourcesHandler().Handle(
                peer,
                new DepositConstructionResourcesRequestEvent(site.GID, new[]
                {
                    new ResourceAmount(ResourceCatalog.WoodId, -1)
                }));

            Assert.That(result.Status, Is.EqualTo(RequestStatus.Rejected));
            Assert.That(SettlementSharedResourcesAccess.GetAmount(storage, ResourceCatalog.WoodId), Is.EqualTo(50));
            Assert.That(ConstructionResourcesAccess.GetDelivered(site, ResourceCatalog.WoodId), Is.EqualTo(0));
            Assert.That(site.Read<ConstructionSiteState>().Phase, Is.EqualTo(ConstructionPhase.WaitingForResources));
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

        private sealed class ConstantHeightSampler : IHeightSampler
        {
            private readonly float _height;

            public ConstantHeightSampler(float height)
            {
                _height = height;
            }

            public float SampleHeight(float worldX, float worldZ) => _height;
        }

        [Test]
        public void ServerStage1FlowSystem_WhenStockpilePlacedEventReceived_AdvancesFromWorkerAssigned()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Stage1SettlementProgressStage.WorkerAssigned);

            var flowSystem = new ServerStage1FlowSystem();
            flowSystem.Init();
            SW.SendEvent(new Stage1StockpilePlacedEvent(SettlementAnchorCatalog.HomeCampId));
            flowSystem.Update();
            flowSystem.Destroy();

            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.StockpilePlaced));
        }

        [Test]
        public void ServerStage1FlowSystem_WhenShelterPlacedEventReceived_AdvancesFromStockpilePlaced()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Stage1SettlementProgressStage.StockpilePlaced);

            var flowSystem = new ServerStage1FlowSystem();
            flowSystem.Init();
            SW.SendEvent(new Stage1ShelterPlacedEvent(SettlementAnchorCatalog.HomeCampId));
            flowSystem.Update();
            flowSystem.Destroy();

            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.ShelterPlaced));
        }

        [Test]
        public void ServerStage1FlowSystem_WhenExtractionOnlineEventReceived_AdvancesFromShelterPlaced()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Stage1SettlementProgressStage.ShelterPlaced);

            var flowSystem = new ServerStage1FlowSystem();
            flowSystem.Init();
            SW.SendEvent(new Stage1ExtractionOnlineEvent(SettlementAnchorCatalog.HomeCampId));
            flowSystem.Update();
            flowSystem.Destroy();

            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.ExtractionOnline));
        }

        [Test]
        public void ServerStage1FlowSystem_WhenWorkbenchOnlineEventReceived_AdvancesFromExtractionOnline()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Stage1SettlementProgressStage.ExtractionOnline);

            var flowSystem = new ServerStage1FlowSystem();
            flowSystem.Init();
            SW.SendEvent(new Stage1WorkbenchOnlineEvent(SettlementAnchorCatalog.HomeCampId));
            flowSystem.Update();
            flowSystem.Destroy();

            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.WorkbenchOnline));
        }

        [Test]
        public void ServerStage1FlowSystem_WhenStockpilePlacedReceivedAtWrongStage_DoesNotAdvance()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Stage1SettlementProgressStage.CampRepaired);

            var flowSystem = new ServerStage1FlowSystem();
            flowSystem.Init();
            SW.SendEvent(new Stage1StockpilePlacedEvent(SettlementAnchorCatalog.HomeCampId));
            flowSystem.Update();
            flowSystem.Destroy();

            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.CampRepaired));
        }

        [Test]
        public void ServerStage1FlowSystem_WhenLoadoutPreparedReceivedAtWorkerAssigned_DoesNotAdvance()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Stage1SettlementProgressStage.WorkerAssigned);

            var flowSystem = new ServerStage1FlowSystem();
            flowSystem.Init();
            SW.SendEvent(new Stage1LoadoutPreparedEvent(SettlementAnchorCatalog.HomeCampId));
            flowSystem.Update();
            flowSystem.Destroy();

            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.WorkerAssigned));
        }

        [Test]
        public void ServerStage1FlowSystem_WhenExtractionOnlineReceivedAtWorkerAssigned_DoesNotAdvance()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Stage1SettlementProgressStage.WorkerAssigned);

            var flowSystem = new ServerStage1FlowSystem();
            flowSystem.Init();
            SW.SendEvent(new Stage1ExtractionOnlineEvent(SettlementAnchorCatalog.HomeCampId));
            flowSystem.Update();
            flowSystem.Destroy();

            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.WorkerAssigned));
        }

        [Test]
        public void ServerStage1FlowSystem_WhenWorkbenchOnlineReceivedAtStockpilePlaced_DoesNotAdvance()
        {
            using var scope = new CombatTestServerWorldScope();
            var anchor = scope.CreateSettlementAnchor(
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Stage1SettlementProgressStage.StockpilePlaced);

            var flowSystem = new ServerStage1FlowSystem();
            flowSystem.Init();
            SW.SendEvent(new Stage1WorkbenchOnlineEvent(SettlementAnchorCatalog.HomeCampId));
            flowSystem.Update();
            flowSystem.Destroy();

            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.StockpilePlaced));
        }
    }
}
