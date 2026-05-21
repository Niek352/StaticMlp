using System;
using NUnit.Framework;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Npc;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Settlement.Workers;
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
        public void ActionCatalog_ResolvesWorkerOwnedExecutors_FromSettlementWorkersAssembly()
        {
            using var scope = new AiTestServerWorldScope();

            var buildExecutor = scope.Catalog.ResolveExecutor(AiTaskType.BuildConstruction);
            var deliveryExecutor = scope.Catalog.ResolveExecutor(AiTaskType.DeliveryResourceToBuilding);
            var followLeaderExecutor = scope.Catalog.ResolveExecutor(AiTaskType.FollowLeader);

            Assert.That(buildExecutor.GetType().Namespace, Is.EqualTo("StaticMlp.Features.Settlement.Workers"));
            Assert.That(deliveryExecutor.GetType().Namespace, Is.EqualTo("StaticMlp.Features.Settlement.Workers"));
            Assert.That(followLeaderExecutor.GetType().Namespace, Is.EqualTo("StaticMlp.Features.AiActions"));
        }

        [Test]
        public void WorkerRoleCatalog_DefinesStage1RoleFlags()
        {
            Assert.That(WorkerRoleCatalog.CampBuilderId, Is.EqualTo(WorkerRoleCatalog.BuilderId));
            Assert.That(WorkerRoleCatalog.All.Count, Is.EqualTo(5));

            AssertRole(
                WorkerRoleCatalog.BuilderId,
                WorkerJobFlags.DeliverConstructionResources
                | WorkerJobFlags.BuildConstruction
                | WorkerJobFlags.MaintainBuildings);
            AssertRole(WorkerRoleCatalog.GathererId, WorkerJobFlags.GatherResources);
            AssertRole(WorkerRoleCatalog.HaulerId, WorkerJobFlags.HaulResources);
            AssertRole(WorkerRoleCatalog.ProcessorId, WorkerJobFlags.ProcessRecipe);
            AssertRole(WorkerRoleCatalog.GuardId, WorkerJobFlags.GuardPost);
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
    }
}
