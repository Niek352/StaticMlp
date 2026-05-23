using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement.Workers
{
    public sealed class ServerSettlementWorkerCampBuilderJobSystem : ISystem
    {
        public void Update()
        {
            foreach (var anchor in SW.Query<All<CampFlowProgression>>().Entities())
            {
                ref readonly var progression = ref anchor.Read<CampFlowProgression>();

                var jobState = CreateJobState(progression.Anchor, in progression);
                var summary = CreateSummary(progression.Anchor, jobState);

                ref var mutableJobState = ref ReplicationMut.Mut<SettlementCampBuilderJobState>(anchor);
                mutableJobState = jobState;

                ref var mutableSummary = ref ReplicationMut.Mut<SettlementWorkerSummary>(anchor);
                mutableSummary = summary;
            }
        }

        private static SettlementCampBuilderJobState CreateJobState(
            SettlementAnchorId anchorId,
            in CampFlowProgression progression)
        {
            var state = new SettlementCampBuilderJobState
            {
                AnchorId = anchorId.Value,
                AssignedWorker = default,
                TargetSite = default,
                CurrentTask = AiTaskType.Idle,
                BlockingReason = SettlementWorkerBlockingReason.None
            };

            if ((byte)progression.Stage < (byte)CampFlowStage.CampRepaired)
            {
                state.AssignedWorker = FindFirstAssignedWorker(anchorId, out _);
                state.BlockingReason = SettlementWorkerBlockingReason.AwaitingCampRepair;
                return state;
            }

            var firstAssignedWorker = default(EntityGID);
            var firstAllowedJobs = WorkerJobFlags.None;

            foreach (var worker in SW.Query<All<SettlementWorkerTag, SettlementWorkerIdentity, SettlementWorkerAssignment>>().Entities())
            {
                ref readonly var identity = ref worker.Read<SettlementWorkerIdentity>();
                if (identity.HomeAnchorId != anchorId.Value)
                    continue;

                ref readonly var assignment = ref worker.Read<SettlementWorkerAssignment>();
                if (!assignment.IsAssigned || assignment.AnchorId != anchorId.Value)
                    continue;

                var role = WorkerRoleCatalog.Get(identity.Role);
                if (!firstAssignedWorker.TryUnpack<ServerWT>(out _))
                {
                    firstAssignedWorker = worker.GID;
                    firstAllowedJobs = role.AllowedJobs;
                }

                if (!SettlementWorkerDemandQuery.TryFindBestDemand(worker, role.AllowedJobs, out var demand))
                    continue;

                state.AssignedWorker = worker.GID;
                state.TargetSite = demand.Target;
                state.CurrentTask = demand.Task;
                state.BlockingReason = SettlementWorkerBlockingReason.None;
                return state;
            }

            if (!firstAssignedWorker.TryUnpack<ServerWT>(out _))
            {
                state.BlockingReason = SettlementWorkerBlockingReason.NoAssignment;
                return state;
            }

            state.AssignedWorker = firstAssignedWorker;
            state.BlockingReason = SettlementWorkerDemandQuery.GetNoDemandReason(firstAllowedJobs);
            return state;
        }

        private static SettlementWorkerSummary CreateSummary(
            SettlementAnchorId anchorId,
            SettlementCampBuilderJobState jobState)
        {
            var summary = new SettlementWorkerSummary
            {
                AnchorId = anchorId.Value,
                ActiveTask = jobState.CurrentTask,
                BlockingReason = jobState.BlockingReason
            };

            foreach (var worker in SW.Query<All<SettlementWorkerTag, SettlementWorkerIdentity, SettlementWorkerAssignment>>().Entities())
            {
                ref readonly var identity = ref worker.Read<SettlementWorkerIdentity>();
                if (identity.HomeAnchorId != anchorId.Value)
                    continue;

                summary.TotalWorkers++;

                if (identity.Role == WorkerRoleCatalog.CampBuilderId)
                    summary.CampBuilderWorkers++;

                ref readonly var assignment = ref worker.Read<SettlementWorkerAssignment>();
                if (!assignment.IsAssigned || assignment.AnchorId != anchorId.Value)
                    continue;

                summary.AssignedWorkers++;

                if (identity.Role == WorkerRoleCatalog.CampBuilderId)
                    summary.CampBuilderAssignedWorkers++;
            }

            if (!jobState.AssignedWorker.TryUnpack<ServerWT>(out _))
                summary.BlockingReason = SettlementWorkerBlockingReason.NoAssignment;

            return summary;
        }

        private static EntityGID FindFirstAssignedWorker(SettlementAnchorId anchorId, out WorkerJobFlags allowedJobs)
        {
            foreach (var worker in SW.Query<All<SettlementWorkerTag, SettlementWorkerIdentity, SettlementWorkerAssignment>>().Entities())
            {
                ref readonly var identity = ref worker.Read<SettlementWorkerIdentity>();
                if (identity.HomeAnchorId != anchorId.Value)
                    continue;

                ref readonly var assignment = ref worker.Read<SettlementWorkerAssignment>();
                if (!assignment.IsAssigned || assignment.AnchorId != anchorId.Value)
                    continue;

                allowedJobs = WorkerRoleCatalog.Get(identity.Role).AllowedJobs;
                return worker.GID;
            }

            allowedJobs = WorkerJobFlags.None;
            return default;
        }
    }
}
