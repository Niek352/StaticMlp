using System;
using StaticMlp.Features.CampFlow;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
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
            PopulateWorkerList(ref state);

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

        private static void PopulateWorkerList(ref WorkerContextPanelState state)
        {
            foreach (var worker in CW.Query<All<SettlementWorkerTag, SettlementWorkerIdentity, SettlementWorkerAssignment>>().Entities())
            {
                ref readonly var identity = ref worker.Read<SettlementWorkerIdentity>();
                if (identity.HomeAnchorId != state.AnchorId.Value)
                    continue;

                if (state.Workers.Length == state.Workers.Capacity)
                    throw new InvalidOperationException(
                        $"{nameof(WorkerContextPanelState)} cannot hold more than {state.Workers.Capacity} worker entries.");

                ref readonly var assignment = ref worker.Read<SettlementWorkerAssignment>();
                var entry = new WorkerListEntryState
                {
                    WorkerId = worker.GID,
                    Role = identity.Role,
                    IsAssigned = assignment.IsAssigned,
                    BlockingReason = SettlementWorkerBlockingReason.None,
                };

                if (worker.Has<BuildingWorkerAssignmentState>())
                {
                    ref readonly var buildingAssignment = ref ClientProjection.Read<BuildingWorkerAssignmentState>(worker);
                    if (buildingAssignment.IsAssigned && buildingAssignment.Building.Raw != 0)
                    {
                        entry.IsAssigned = true;
                        entry.IsBuildingAssignment = true;
                        entry.Building = buildingAssignment.Building;
                        entry.SlotIndex = buildingAssignment.SlotIndex;
                    }
                }

                if (assignment.IsAssigned && identity.Role == WorkerRoleCatalog.CampBuilderId)
                {
                    if (CampFlowProgressionQuery.TryGetClientAnchor(state.AnchorId, out var anchor)
                        && anchor.Has<Projected<SettlementCampBuilderJobState>>())
                    {
                        ref readonly var job = ref ClientProjection.Read<SettlementCampBuilderJobState>(anchor);
                        entry.BlockingReason = job.BlockingReason;
                    }
                }

                state.Workers.Add(entry);
            }
        }

    }
}
