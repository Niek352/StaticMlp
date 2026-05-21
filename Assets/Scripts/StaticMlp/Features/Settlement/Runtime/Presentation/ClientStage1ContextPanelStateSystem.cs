using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public sealed class ClientStage1ContextPanelStateSystem : ISystem
    {
        public void Update()
        {
            ref readonly var session = ref CW.GetResource<Stage1ContextPanelSession>();
            var next = new Stage1ContextPanelState
            {
                Mode = session.Mode,
                AnchorId = session.WorkerAnchorId,
                FocusedSite = session.FocusedSite,
            };

            if (session.Mode == Stage1ContextPanelMode.Building)
            {
                if (!TryGetConstructionSite(session.FocusedSite, out var site))
                    return;

                ref readonly var state = ref ClientProjection.Read<ConstructionSiteState>(site);
                ref readonly var progress = ref ClientProjection.Read<ConstructionProgress>(site);

                next.HasFocusedSite = true;
                next.ConstructionPhase = state.Phase;
                next.WoodRequired = ConstructionResourcesAccess.GetProjectedRequired(site, ResourceCatalog.WoodId);
                next.StoneRequired = ConstructionResourcesAccess.GetProjectedRequired(site, ResourceCatalog.StoneId);
                next.WoodDelivered = ConstructionResourcesAccess.GetProjectedDelivered(site, ResourceCatalog.WoodId);
                next.StoneDelivered = ConstructionResourcesAccess.GetProjectedDelivered(site, ResourceCatalog.StoneId);
                next.Progress01 = progress.Normalized;
                next.CanDepositResources = !ConstructionResourcesAccess.IsProjectedComplete(site);
                next.CanBuild = ConstructionResourcesAccess.IsProjectedComplete(site)
                                && state.Phase is ConstructionPhase.ReadyToBuild or ConstructionPhase.BuildingInProgress;
                CW.SetResource(next);
                return;
            }

            next.Mode = Stage1ContextPanelMode.Worker;
            PopulateWorkerState(ref next);

            CW.SetResource(next);
        }

        private static void PopulateWorkerState(ref Stage1ContextPanelState state)
        {
            if (Stage1SettlementProgressionQuery.TryGetClientAnchor(state.AnchorId, out var anchor))
            {
                ref readonly var flow = ref ClientProjection.Read<Stage1FlowViewState>(anchor);
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

        private static bool TryGetConstructionSite(EntityGID gid, out CW.Entity site)
        {
            if (!gid.TryUnpack<ClientCoreWT>(out site))
                return false;

            return site.Has<ConstructionSiteState>();
        }
    }
}
