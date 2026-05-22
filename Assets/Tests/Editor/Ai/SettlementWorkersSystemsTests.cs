using System;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Npc;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;
using UnityEngine;

namespace StaticMlp.Tests.Ai
{
    public sealed class SettlementWorkersSystemsTests
    {
        [Test]
        public void AssignmentHandler_RejectsBeforeCampRepair_AndAssignsAfterRepair()
        {
            using var scope = new AiTestServerWorldScope();
            var anchorId = SettlementAnchorCatalog.HomeCampId;
            var anchor = scope.CreateSettlementAnchor(anchorId, Stage1SettlementProgressStage.RepairResourcesReady);
            var worker = scope.CreateWorker(anchorId, new Vector3(1f, 0f, 1f));
            var handler = new SetSettlementWorkerAssignmentHandler();
            var request = new SetSettlementWorkerAssignmentRequestEvent(worker.GID, anchorId, assigned: true);

            var rejected = handler.Handle(default, in request);

            Assert.That(rejected.Status, Is.EqualTo(RequestStatus.Rejected));
            Assert.That(worker.Read<SettlementWorkerAssignment>().Status, Is.EqualTo(SettlementWorkerAssignmentStatus.Unassigned));

            anchor.Set(new Stage1SettlementProgression
            {
                AnchorId = anchorId.Value,
                Stage = Stage1SettlementProgressStage.CampRepaired
            });

            var accepted = handler.Handle(default, in request);

            Assert.That(accepted.Status, Is.EqualTo(RequestStatus.Accepted));
            Assert.That(worker.Read<SettlementWorkerAssignment>().Status, Is.EqualTo(SettlementWorkerAssignmentStatus.Assigned));
            Assert.That(worker.Read<SettlementWorkerAssignment>().AnchorId, Is.EqualTo(anchorId.Value));
        }

        [Test]
        public void CampBuilderJobSystem_BuildsSummaryWithoutMutatingStage()
        {
            using var scope = new AiTestServerWorldScope();
            var anchorId = SettlementAnchorCatalog.HomeCampId;
            var anchor = scope.CreateSettlementAnchor(anchorId, Stage1SettlementProgressStage.CampRepaired);
            var worker = scope.CreateWorker(anchorId, Vector3.zero, SettlementWorkerAssignmentStatus.Assigned);
            var site = scope.CreateConstructionSite(new Vector3(2f, 0f, 0f), ConstructionPhase.WaitingForResources, resourcesComplete: false);

            new ServerSettlementWorkerCampBuilderJobSystem().Update();

            Assert.That(anchor.Read<Stage1SettlementProgression>().Stage, Is.EqualTo(Stage1SettlementProgressStage.CampRepaired));

            ref readonly var job = ref anchor.Read<SettlementCampBuilderJobState>();
            Assert.That(job.AssignedWorker, Is.EqualTo(worker.GID));
            Assert.That(job.TargetSite, Is.EqualTo(site.GID));
            Assert.That(job.CurrentTask, Is.EqualTo(AiTaskType.DeliveryResourceToBuilding));
            Assert.That(job.BlockingReason, Is.EqualTo(SettlementWorkerBlockingReason.None));

            ref readonly var summary = ref anchor.Read<SettlementWorkerSummary>();
            Assert.That(summary.TotalWorkers, Is.EqualTo(1));
            Assert.That(summary.AssignedWorkers, Is.EqualTo(1));
            Assert.That(summary.CampBuilderWorkers, Is.EqualTo(1));
            Assert.That(summary.CampBuilderAssignedWorkers, Is.EqualTo(1));
            Assert.That(summary.ActiveTask, Is.EqualTo(AiTaskType.DeliveryResourceToBuilding));
            Assert.That(summary.BlockingReason, Is.EqualTo(SettlementWorkerBlockingReason.None));
        }

        [Test]
        public void BuilderJobSystem_PrefersDeliveryOverHaul()
        {
            using var scope = new AiTestServerWorldScope();
            var anchorId = SettlementAnchorCatalog.HomeCampId;
            var anchor = scope.CreateSettlementAnchor(anchorId, Stage1SettlementProgressStage.CampRepaired);
            var worker = scope.CreateWorker(anchorId, Vector3.zero, SettlementWorkerAssignmentStatus.Assigned, WorkerRoleCatalog.BuilderId);
            var site = scope.CreateConstructionSite(new Vector3(2f, 0f, 0f), ConstructionPhase.WaitingForResources, resourcesComplete: false);
            CreateWorkbench(outputPlanks: 5);
            CreateStockpile();

            new ServerSettlementWorkerCampBuilderJobSystem().Update();

            // Builder must prefer ConstructionDelivery over HaulResources even when haul demand is also available.
            // This regression test guards against priority changes after large refactors of CreateJobState.
            ref readonly var job = ref anchor.Read<SettlementCampBuilderJobState>();
            Assert.That(job.AssignedWorker, Is.EqualTo(worker.GID));
            Assert.That(job.CurrentTask, Is.EqualTo(AiTaskType.DeliveryResourceToBuilding));
            Assert.That(job.TargetSite, Is.EqualTo(site.GID));
        }

        [Test]
        public void DemandQuery_ReturnsTypedResourceDataForWorkbenchDemands()
        {
            using var scope = new AiTestServerWorldScope();
            var gatherWorkbench = CreateWorkbench(inputWood: 0);
            var processWorkbench = CreateWorkbench(inputWood: 2);
            var haulWorkbench = CreateWorkbench(outputPlanks: 3);
            CreateStockpile();

            Assert.That(SettlementWorkerDemandQuery.TryFindGatherDemand(out var gatherDemand), Is.True);
            Assert.That(gatherDemand.Kind, Is.EqualTo(SettlementWorkerDemand.DemandKind.Gather));
            Assert.That(gatherDemand.Task, Is.EqualTo(AiTaskType.GatherResources));
            Assert.That(gatherDemand.Target, Is.EqualTo(gatherWorkbench.GID));
            Assert.That(gatherDemand.Resource, Is.EqualTo(ResourceCatalog.WoodId));
            Assert.That(gatherDemand.Amount, Is.EqualTo(2));

            Assert.That(SettlementWorkerDemandQuery.TryFindProcessDemand(out var processDemand), Is.True);
            Assert.That(processDemand.Kind, Is.EqualTo(SettlementWorkerDemand.DemandKind.Process));
            Assert.That(processDemand.Task, Is.EqualTo(AiTaskType.ProcessRecipe));
            Assert.That(processDemand.Target, Is.EqualTo(processWorkbench.GID));
            Assert.That(processDemand.Resource, Is.EqualTo(ResourceCatalog.PlanksId));
            Assert.That(processDemand.Amount, Is.EqualTo(1));

            Assert.That(SettlementWorkerDemandQuery.TryFindHaulDemand(out var haulDemand), Is.True);
            Assert.That(haulDemand.Kind, Is.EqualTo(SettlementWorkerDemand.DemandKind.Haul));
            Assert.That(haulDemand.Task, Is.EqualTo(AiTaskType.HaulResources));
            Assert.That(haulDemand.Target, Is.EqualTo(haulWorkbench.GID));
            Assert.That(haulDemand.Resource, Is.EqualTo(ResourceCatalog.PlanksId));
            Assert.That(haulDemand.Amount, Is.EqualTo(3));
        }

        [Test]
        public void DemandQuery_PrioritizesExtractionHaulDemandOverWorkbenchOutput()
        {
            using var scope = new AiTestServerWorldScope();
            var extraction = CreateExtractionBuilding(ResourceCatalog.StoneId, outputAmount: 7);
            CreateWorkbench(outputPlanks: 3);
            CreateStockpile();

            Assert.That(SettlementWorkerDemandQuery.TryFindHaulDemand(out var demand), Is.True);
            Assert.That(demand.Kind, Is.EqualTo(SettlementWorkerDemand.DemandKind.Haul));
            Assert.That(demand.Task, Is.EqualTo(AiTaskType.HaulResources));
            Assert.That(demand.Target, Is.EqualTo(extraction.GID));
            Assert.That(demand.Resource, Is.EqualTo(ResourceCatalog.StoneId));
            Assert.That(demand.Amount, Is.EqualTo(7));
        }

        [Test]
        public void GathererJobSystem_SelectsGatherDemand_NotConstructionDemand()
        {
            using var scope = new AiTestServerWorldScope();
            var anchorId = SettlementAnchorCatalog.HomeCampId;
            var anchor = scope.CreateSettlementAnchor(anchorId, Stage1SettlementProgressStage.CampRepaired);
            var worker = scope.CreateWorker(
                anchorId,
                Vector3.zero,
                SettlementWorkerAssignmentStatus.Assigned,
                WorkerRoleCatalog.GathererId);
            var workbench = CreateWorkbench(inputWood: 0);
            scope.CreateConstructionSite(new Vector3(1f, 0f, 0f), ConstructionPhase.WaitingForResources, resourcesComplete: false);

            new ServerSettlementWorkerCampBuilderJobSystem().Update();

            ref readonly var job = ref anchor.Read<SettlementCampBuilderJobState>();
            Assert.That(job.AssignedWorker, Is.EqualTo(worker.GID));
            Assert.That(job.TargetSite, Is.EqualTo(workbench.GID));
            Assert.That(job.CurrentTask, Is.EqualTo(AiTaskType.GatherResources));
            Assert.That(job.BlockingReason, Is.EqualTo(SettlementWorkerBlockingReason.None));
        }

        [Test]
        public void HaulerJobSystem_SelectsHaulDemand_NotGatherDemand()
        {
            using var scope = new AiTestServerWorldScope();
            var anchorId = SettlementAnchorCatalog.HomeCampId;
            var anchor = scope.CreateSettlementAnchor(anchorId, Stage1SettlementProgressStage.CampRepaired);
            var worker = scope.CreateWorker(
                anchorId,
                Vector3.zero,
                SettlementWorkerAssignmentStatus.Assigned,
                WorkerRoleCatalog.HaulerId);
            CreateWorkbench(inputWood: 0);
            var outputWorkbench = CreateWorkbench(outputPlanks: 3);
            CreateStockpile();

            new ServerSettlementWorkerCampBuilderJobSystem().Update();

            ref readonly var job = ref anchor.Read<SettlementCampBuilderJobState>();
            Assert.That(job.AssignedWorker, Is.EqualTo(worker.GID));
            Assert.That(job.TargetSite, Is.EqualTo(outputWorkbench.GID));
            Assert.That(job.CurrentTask, Is.EqualTo(AiTaskType.HaulResources));
            Assert.That(job.BlockingReason, Is.EqualTo(SettlementWorkerBlockingReason.None));
        }

        [Test]
        public void ProcessorJobSystem_SelectsProcessDemand()
        {
            using var scope = new AiTestServerWorldScope();
            var anchorId = SettlementAnchorCatalog.HomeCampId;
            var anchor = scope.CreateSettlementAnchor(anchorId, Stage1SettlementProgressStage.CampRepaired);
            var worker = scope.CreateWorker(
                anchorId,
                Vector3.zero,
                SettlementWorkerAssignmentStatus.Assigned,
                WorkerRoleCatalog.ProcessorId);
            var workbench = CreateWorkbench(inputWood: 2);

            new ServerSettlementWorkerCampBuilderJobSystem().Update();

            ref readonly var job = ref anchor.Read<SettlementCampBuilderJobState>();
            Assert.That(job.AssignedWorker, Is.EqualTo(worker.GID));
            Assert.That(job.TargetSite, Is.EqualTo(workbench.GID));
            Assert.That(job.CurrentTask, Is.EqualTo(AiTaskType.ProcessRecipe));
            Assert.That(job.BlockingReason, Is.EqualTo(SettlementWorkerBlockingReason.None));
        }

        [Test]
        public void GuardJobSystem_IgnoresEconomyDemandWithoutAllowedJobFlags()
        {
            using var scope = new AiTestServerWorldScope();
            var anchorId = SettlementAnchorCatalog.HomeCampId;
            var anchor = scope.CreateSettlementAnchor(anchorId, Stage1SettlementProgressStage.CampRepaired);
            var worker = scope.CreateWorker(
                anchorId,
                Vector3.zero,
                SettlementWorkerAssignmentStatus.Assigned,
                WorkerRoleCatalog.GuardId);
            CreateWorkbench(inputWood: 0, outputPlanks: 3);
            CreateStockpile();
            scope.CreateConstructionSite(new Vector3(1f, 0f, 0f), ConstructionPhase.WaitingForResources, resourcesComplete: false);

            new ServerSettlementWorkerCampBuilderJobSystem().Update();

            ref readonly var job = ref anchor.Read<SettlementCampBuilderJobState>();
            Assert.That(job.AssignedWorker, Is.EqualTo(worker.GID));
            Assert.That(job.TargetSite, Is.EqualTo(default(EntityGID)));
            Assert.That(job.CurrentTask, Is.EqualTo(AiTaskType.Idle));
            Assert.That(job.BlockingReason, Is.EqualTo(SettlementWorkerBlockingReason.NoEligibleDemand));
        }

        [Test]
        public void TaskSyncSystem_PushesCampOwnedTargetIntoExistingAiExecutionState()
        {
            using var scope = new AiTestServerWorldScope();
            var anchorId = SettlementAnchorCatalog.HomeCampId;
            var anchor = scope.CreateSettlementAnchor(anchorId, Stage1SettlementProgressStage.WorkerAssigned);
            var worker = scope.CreateWorker(anchorId, Vector3.zero, SettlementWorkerAssignmentStatus.Assigned);
            var site = scope.CreateConstructionSite(new Vector3(3f, 0f, 0f), ConstructionPhase.ReadyToBuild, resourcesComplete: true);

            anchor.Set(new SettlementCampBuilderJobState
            {
                AnchorId = anchorId.Value,
                AssignedWorker = worker.GID,
                TargetSite = site.GID,
                CurrentTask = AiTaskType.BuildConstruction,
                BlockingReason = SettlementWorkerBlockingReason.None
            });

            new ServerSettlementWorkerTaskSyncSystem().Update();

            Assert.That(worker.Read<AiTaskState>().Task, Is.EqualTo(AiTaskType.BuildConstruction));
            Assert.That(AiBlackboardAccess.TryGetEntity(worker, BuildConstructionCollectVariables.BuildTargetSite, out var target), Is.True);
            Assert.That(target, Is.EqualTo(site.GID));
            Assert.That(AiBlackboardAccess.TryGetEntity(worker, DeliveryBuildResourcesCollectVariables.TargetSite, out _), Is.False);
        }

        [Test]
        public void TaskSyncSystem_SwitchesAssignedWorkerToEconomyDemandTask()
        {
            using var scope = new AiTestServerWorldScope();
            var anchorId = SettlementAnchorCatalog.HomeCampId;
            var anchor = scope.CreateSettlementAnchor(anchorId, Stage1SettlementProgressStage.WorkerAssigned);
            var worker = scope.CreateWorker(
                anchorId,
                Vector3.zero,
                SettlementWorkerAssignmentStatus.Assigned,
                WorkerRoleCatalog.HaulerId);
            var workbench = CreateWorkbench(outputPlanks: 3);

            anchor.Set(new SettlementCampBuilderJobState
            {
                AnchorId = anchorId.Value,
                AssignedWorker = worker.GID,
                TargetSite = workbench.GID,
                CurrentTask = AiTaskType.HaulResources,
                BlockingReason = SettlementWorkerBlockingReason.None
            });

            new ServerSettlementWorkerTaskSyncSystem().Update();

            Assert.That(worker.Read<AiTaskState>().Task, Is.EqualTo(AiTaskType.HaulResources));
            Assert.That(AiBlackboardAccess.TryGetEntity(worker, HaulExtractionOutputCollectVariables.TargetExtractionBuilding, out var target), Is.True);
            Assert.That(target, Is.EqualTo(workbench.GID));
            Assert.That(AiBlackboardAccess.TryGetEntity(worker, BuildConstructionCollectVariables.BuildTargetSite, out _), Is.False);
            Assert.That(AiBlackboardAccess.TryGetEntity(worker, DeliveryBuildResourcesCollectVariables.TargetSite, out _), Is.False);
        }

        [Test]
        public void HaulExtractionOutputExecutor_EmitsSettlementOwnedTransferIntent()
        {
            using var scope = new AiTestServerWorldScope();
            var worker = scope.CreateWorker(
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                SettlementWorkerAssignmentStatus.Assigned,
                WorkerRoleCatalog.HaulerId);
            var extraction = CreateExtractionBuilding(ResourceCatalog.WoodId, outputAmount: 6);
            var receiver = SW.RegisterEventReceiver<TransferExtractionOutputToStockpileEvent>();
            AiBlackboardAccess.SetEntity(worker, HaulExtractionOutputCollectVariables.TargetExtractionBuilding, extraction.GID);

            ref var task = ref worker.Mut<AiTaskState>();
            task.Task = AiTaskType.HaulResources;
            new HaulExtractionOutputExecutor(new AiTaskExecutionTransitions()).Execute(worker, ref task);

            var received = false;
            foreach (var e in receiver)
            {
                received = true;
                Assert.That(e.Value.ExtractionBuilding, Is.EqualTo(extraction.GID));
                Assert.That(e.Value.Resource, Is.EqualTo(ResourceCatalog.WoodId));
                Assert.That(e.Value.RequestedAmount, Is.EqualTo(6));
            }

            SW.DeleteEventReceiver(ref receiver);
            Assert.That(received, Is.True);
            Assert.That(extraction.Read<ExtractionOperationState>().OutputBufferAmount, Is.EqualTo(6));
        }

        [Test]
        public void ActionCatalog_ResolvesWorkerOwnedExecutors_FromSettlementWorkersAssembly()
        {
            using var scope = new AiTestServerWorldScope();

            var buildExecutor = scope.Catalog.ResolveExecutor(AiTaskType.BuildConstruction);
            var deliveryExecutor = scope.Catalog.ResolveExecutor(AiTaskType.DeliveryResourceToBuilding);
            var haulExecutor = scope.Catalog.ResolveExecutor(AiTaskType.HaulResources);
            var followLeaderExecutor = scope.Catalog.ResolveExecutor(AiTaskType.FollowLeader);

            Assert.That(buildExecutor.GetType().Namespace, Is.EqualTo("StaticMlp.Features.Settlement.Workers"));
            Assert.That(deliveryExecutor.GetType().Namespace, Is.EqualTo("StaticMlp.Features.Settlement.Workers"));
            Assert.That(haulExecutor.GetType().Namespace, Is.EqualTo("StaticMlp.Features.Settlement.Workers"));
            Assert.That(followLeaderExecutor.GetType().Namespace, Is.EqualTo("StaticMlp.Features.AiActions"));
        }

        [Test]
        public void WorkerRoleCatalog_DefinesStage1RoleFlags()
        {
            Assert.That(WorkerRoleCatalog.CampBuilderId, Is.EqualTo(WorkerRoleCatalog.BuilderId));
            Assert.That(WorkerRoleCatalog.All.Count, Is.EqualTo(5));

            // Verify each role resolves from the catalog and carries the required capabilities.
            // Flags are read from the catalog (single source of truth) rather than re-composed here.
            var builderJobs = WorkerRoleCatalog.Get(WorkerRoleCatalog.BuilderId).AllowedJobs;
            Assert.That((builderJobs & WorkerJobFlags.DeliverConstructionResources) != 0, Is.True, "Builder must allow DeliverConstructionResources");
            Assert.That((builderJobs & WorkerJobFlags.BuildConstruction) != 0, Is.True, "Builder must allow BuildConstruction");
            Assert.That((builderJobs & WorkerJobFlags.MaintainBuildings) != 0, Is.True, "Builder must allow MaintainBuildings");

            Assert.That(WorkerRoleCatalog.Get(WorkerRoleCatalog.GathererId).AllowedJobs, Is.EqualTo(WorkerJobFlags.GatherResources));
            Assert.That(WorkerRoleCatalog.Get(WorkerRoleCatalog.HaulerId).AllowedJobs, Is.EqualTo(WorkerJobFlags.HaulResources));
            Assert.That(WorkerRoleCatalog.Get(WorkerRoleCatalog.ProcessorId).AllowedJobs, Is.EqualTo(WorkerJobFlags.ProcessRecipe));
            Assert.That(WorkerRoleCatalog.Get(WorkerRoleCatalog.GuardId).AllowedJobs, Is.EqualTo(WorkerJobFlags.GuardPost));
        }

        [Test]
        public void RuntimeProfileCatalog_DefinesStage1WorkerRoles()
        {
            var catalog = AiActionCatalog.Discover(new AiTaskExecutionTransitions());

            AssertRuntimeProfile(
                WorkerRoleCatalog.BuilderId,
                SettlementWorkerBehaviorIds.PEACEFUL_BUILDER,
                maxHealth: 100f);
            AssertRuntimeProfile(
                WorkerRoleCatalog.GathererId,
                SettlementWorkerBehaviorIds.PEACEFUL_GATHERER,
                maxHealth: 90f);
            AssertRuntimeProfile(
                WorkerRoleCatalog.HaulerId,
                SettlementWorkerBehaviorIds.PEACEFUL_HAULER,
                maxHealth: 95f);
            AssertRuntimeProfile(
                WorkerRoleCatalog.ProcessorId,
                SettlementWorkerBehaviorIds.PEACEFUL_PROCESSOR,
                maxHealth: 85f);
            AssertRuntimeProfile(
                WorkerRoleCatalog.GuardId,
                SettlementWorkerBehaviorIds.SETTLEMENT_GUARD,
                maxHealth: 100f);

            AssertBehavior(catalog, SettlementWorkerBehaviorIds.PEACEFUL_BUILDER);
            AssertBehavior(catalog, SettlementWorkerBehaviorIds.PEACEFUL_GATHERER);
            AssertBehavior(catalog, SettlementWorkerBehaviorIds.PEACEFUL_HAULER);
            AssertBehavior(catalog, SettlementWorkerBehaviorIds.PEACEFUL_PROCESSOR);
            AssertBehavior(catalog, SettlementWorkerBehaviorIds.SETTLEMENT_GUARD);
        }

        [Test]
        public void WorkerNpcProfileCatalog_MapsStage1WorkerRoles()
        {
            AssertNpcProfile(WorkerRoleCatalog.BuilderId, NpcDefinitionCatalog.SeededBuilderId, NpcRoleFlags.Builder);
            AssertNpcProfile(WorkerRoleCatalog.GathererId, NpcDefinitionCatalog.SeededGathererId, NpcRoleFlags.Gatherer);
            AssertNpcProfile(WorkerRoleCatalog.HaulerId, NpcDefinitionCatalog.SeededHaulerId, NpcRoleFlags.Hauler);
            AssertNpcProfile(WorkerRoleCatalog.ProcessorId, NpcDefinitionCatalog.SeededProcessorId, NpcRoleFlags.Processor);
            AssertNpcProfile(WorkerRoleCatalog.GuardId, NpcDefinitionCatalog.SeededGuardId, NpcRoleFlags.Guard);
        }

        [Test]
        public void SpawnedWorker_HasNpcTag()
        {
            using var scope = new AiTestServerWorldScope();
            var worker = scope.CreateWorker(SettlementAnchorCatalog.HomeCampId, Vector3.zero);

            Assert.That(worker.Has<NpcTag>(), Is.True);
        }

        [Test]
        public void SpawnedWorker_HasNpcIdentity()
        {
            using var scope = new AiTestServerWorldScope();
            var worker = scope.CreateWorker(SettlementAnchorCatalog.HomeCampId, Vector3.zero);

            Assert.That(worker.Has<NpcIdentity>(), Is.True);

            ref readonly var identity = ref worker.Read<NpcIdentity>();
            Assert.That(identity.DefinitionId, Is.EqualTo(NpcDefinitionCatalog.SeededCampBuilderId.Value));
            Assert.That(identity.Class, Is.EqualTo(NpcClass.Companion));
            Assert.That(identity.AcquisitionPath, Is.EqualTo(NpcAcquisitionPath.Seeded));
            Assert.That(identity.Roles, Is.EqualTo(NpcRoleFlags.Builder));
        }

        [Test]
        public void MissingWorkerToNpcMapping_Throws()
        {
            Assert.Throws<InvalidOperationException>(() => SettlementWorkerNpcProfileCatalog.Get(new WorkerRoleId(999)));
        }

        private static void AssertRole(WorkerRoleId roleId, WorkerJobFlags allowedJobs)
        {
            var definition = WorkerRoleCatalog.Get(roleId);

            Assert.That(definition.Id, Is.EqualTo(roleId));
            Assert.That(definition.AllowedJobs, Is.EqualTo(allowedJobs));
        }

        private static void AssertRuntimeProfile(WorkerRoleId roleId, ushort behaviorId, float maxHealth)
        {
            var profile = SettlementWorkerRuntimeProfileCatalog.Get(roleId);

            Assert.That(profile.RoleId, Is.EqualTo(roleId));
            Assert.That(profile.NetworkArchetypeId, Is.EqualTo(SettlementWorkerNetworkArchetypeIds.SETTLEMENT_WORKER));
            Assert.That(profile.BehaviorId, Is.EqualTo(behaviorId));
            Assert.That(profile.MaxHealth, Is.EqualTo(maxHealth));
        }

        private static void AssertBehavior(AiActionCatalog catalog, ushort behaviorId)
        {
            Assert.That(catalog.TryGetBehavior(behaviorId, out _), Is.True);
        }

        private static void AssertNpcProfile(WorkerRoleId roleId, NpcDefinitionId definitionId, NpcRoleFlags roles)
        {
            var mappedDefinitionId = SettlementWorkerNpcProfileCatalog.Get(roleId);
            var definition = NpcDefinitionCatalog.Get(mappedDefinitionId);

            Assert.That(mappedDefinitionId, Is.EqualTo(definitionId));
            Assert.That(definition.Roles, Is.EqualTo(roles));
            Assert.That(definition.AllowedAcquisitionPaths, Is.EqualTo(NpcAcquisitionPathFlags.Seeded));
        }

        private static SW.Entity CreateWorkbench(
            int inputWood = 0,
            int outputPlanks = 0,
            bool enabled = true)
        {
            var entity = SW.NewEntity<Default>();
            entity.Set(new WorkbenchOperationState
            {
                ActiveRecipeId = WorkbenchRecipeCatalog.PlanksId.Value,
                Enabled = enabled,
                WorkerSlotCount = 1,
                WorkDone = 0f
            });
            WorkbenchResourceAccess.InitializeRows(entity);
            ref var inputs = ref entity.Ref<SW.Multi<WorkbenchInputResource>>();
            SetWorkbenchInput(ref inputs, ResourceCatalog.WoodId, inputWood);
            ref var outputs = ref entity.Ref<SW.Multi<WorkbenchOutputResource>>();
            SetWorkbenchOutput(ref outputs, ResourceCatalog.PlanksId, outputPlanks);
            return entity;
        }

        private static void SetWorkbenchInput(ref SW.Multi<WorkbenchInputResource> rows, ResourceId resourceId, int amount)
        {
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Id != resourceId)
                    continue;

                ref var row = ref rows[i];
                row.Amount = amount;
                return;
            }

            throw new InvalidOperationException($"Missing workbench input resource id {resourceId.Value}.");
        }

        private static void SetWorkbenchOutput(ref SW.Multi<WorkbenchOutputResource> rows, ResourceId resourceId, int amount)
        {
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Id != resourceId)
                    continue;

                ref var row = ref rows[i];
                row.Amount = amount;
                return;
            }

            throw new InvalidOperationException($"Missing workbench output resource id {resourceId.Value}.");
        }

        private static SW.Entity CreateStockpile(bool enabled = true)
        {
            var entity = SW.NewEntity<Default>();
            entity.Set(new StockpileOperationState
            {
                Enabled = enabled,
                ContributedCapacity = 200
            });
            return entity;
        }

        private static SW.Entity CreateExtractionBuilding(ResourceId outputResource, int outputAmount, bool enabled = true)
        {
            var entity = SW.NewEntity<Default>();
            entity.Set<FinishedBuildingTag>();
            entity.Set(new ConstructionTransform
            {
                Position = Vector3.zero,
                Rotation = Quaternion.identity
            });
            entity.Set(new ExtractionOperationState
            {
                OutputResourceId = outputResource.Value,
                OutputBufferAmount = outputAmount,
                OutputBufferCapacity = 40,
                Enabled = enabled,
                WorkerSlotCount = 2
            });
            return entity;
        }
    }
}
