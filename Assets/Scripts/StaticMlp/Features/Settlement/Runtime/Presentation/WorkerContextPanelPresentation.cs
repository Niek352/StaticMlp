using StaticMlp.Features.CampFlow;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public static class WorkerContextPanelPresentation
    {
        public static WorkerContextPanelState Build(in SettlementContextPanelSession session)
        {
            if (session.Mode != SettlementContextPanelMode.Worker)
                return default;

            var state = new WorkerContextPanelState
            {
                AnchorId = session.WorkerAnchorId,
            };

            PopulateWorkerState(ref state);

            return state;
        }

        private static void PopulateWorkerState(ref WorkerContextPanelState state)
        {
            if (CampFlowProgressionQuery.TryGetClientAnchor(state.AnchorId, out var anchor))
            {
                ref readonly var flow = ref ClientProjection.Read<CampFlowViewState>(anchor);
                if (anchor.Has<Projected<SettlementWorkerSummary>>())
                {
                    ref readonly var summary = ref ClientProjection.Read<SettlementWorkerSummary>(anchor);
                    state.WorkerBlockingReason = summary.BlockingReason;
                }

                if (anchor.Has<Projected<SettlementCampBuilderJobState>>())
                {
                    ref readonly var job = ref ClientProjection.Read<SettlementCampBuilderJobState>(anchor);
                    state.WorkerId = job.AssignedWorker;
                    state.HasWorker = job.AssignedWorker.Raw != 0;
                    state.WorkerAssigned = job.AssignedWorker.Raw != 0;
                    state.WorkerActiveTask = job.CurrentTask;
                    state.WorkerBlockingReason = job.BlockingReason;
                }

                state.CanToggleWorkerAssignment = flow.CanToggleWorkerAssignment;
            }

            foreach (var worker in CW.Query<All<SettlementWorkerTag, SettlementWorkerIdentity, SettlementWorkerAssignment>>().Entities())
            {
                ref readonly var identity = ref worker.Read<SettlementWorkerIdentity>();
                if (identity.HomeAnchorId != state.AnchorId.Value)
                    continue;

                ref readonly var assignment = ref worker.Read<SettlementWorkerAssignment>();
                state.WorkerId = worker.GID;
                state.HasWorker = true;
                state.WorkerAssigned = assignment.IsAssigned;

                if (!state.WorkerAssigned)
                    state.WorkerBlockingReason = SettlementWorkerBlockingReason.NoAssignment;

                return;
            }
        }
    }
}
