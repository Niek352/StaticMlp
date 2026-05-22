using System;
using System.Text;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;
using Unity.Collections;

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
                var definition = BuildingCatalogData.Get(new BuildingId(state.BuildingId));

                next.HasFocusedSite = true;
                next.BuildingDisplayName = definition.DisplayName;
                next.ConstructionPhase = state.Phase;
                CopyProjectedConstructionResources(site, ref next.ConstructionResources);
                next.Progress01 = progress.Normalized;
                next.PrimaryBuildingAction = CreatePrimaryBuildingAction(site, in state, in definition);
                next.SecondaryBuildingAction = CreateSecondaryBuildingAction(site.GID);
                PopulateOpenedBuildingAction(ref next, site.GID, in definition);
                next.CanDepositResources = next.PrimaryBuildingAction.Kind == BuildingInteractionKind.DepositConstructionResources
                                           && next.PrimaryBuildingAction.Enabled;
                next.CanBuild = next.PrimaryBuildingAction.Kind == BuildingInteractionKind.ContributeBuildWork
                                && next.PrimaryBuildingAction.Enabled;
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

        private static BuildingAvailableActionPresentation CreatePrimaryBuildingAction(
            CW.Entity site,
            in ConstructionSiteState state,
            in BuildingDefinition definition)
        {
            if (state.Phase == ConstructionPhase.Completed)
            {
                var kind = SelectCompletedPrimaryAction(in definition);
                return new BuildingAvailableActionPresentation(
                    kind,
                    ResolveLabel(kind),
                    enabled: true,
                    disabledReason: string.Empty,
                    target: site.GID);
            }

            var resourcesComplete = ConstructionResourcesAccess.IsProjectedComplete(site);
            if (!resourcesComplete)
            {
                RequireInteraction(in definition, BuildingInteractionKind.DepositConstructionResources);
                var enabled = SettlementConstructionRules.CanDepositResources(in state);
                return new BuildingAvailableActionPresentation(
                    BuildingInteractionKind.DepositConstructionResources,
                    ResolveLabel(BuildingInteractionKind.DepositConstructionResources),
                    enabled,
                    enabled ? string.Empty : "Construction is not waiting for resources.",
                    target: site.GID);
            }

            RequireInteraction(in definition, BuildingInteractionKind.ContributeBuildWork);
            var canBuild = SettlementConstructionRules.CanProjectedBuild(in state, site);
            return new BuildingAvailableActionPresentation(
                BuildingInteractionKind.ContributeBuildWork,
                ResolveLabel(BuildingInteractionKind.ContributeBuildWork),
                canBuild,
                canBuild ? string.Empty : "Construction is not ready for build work.",
                target: site.GID);
        }

        private static BuildingAvailableActionPresentation CreateSecondaryBuildingAction(EntityGID target)
        {
            return new BuildingAvailableActionPresentation(
                BuildingInteractionKind.OpenDetails,
                ResolveLabel(BuildingInteractionKind.OpenDetails),
                enabled: true,
                disabledReason: string.Empty,
                target: target);
        }

        private static BuildingInteractionKind SelectCompletedPrimaryAction(in BuildingDefinition definition)
        {
            if (HasInteraction(in definition, BuildingInteractionKind.OpenProductionQueue))
                return BuildingInteractionKind.OpenProductionQueue;
            if (HasInteraction(in definition, BuildingInteractionKind.StoreItems))
                return BuildingInteractionKind.StoreItems;
            if (HasInteraction(in definition, BuildingInteractionKind.AssignBed))
                return BuildingInteractionKind.AssignBed;
            if (HasInteraction(in definition, BuildingInteractionKind.Extract))
                return BuildingInteractionKind.Extract;
            if (HasInteraction(in definition, BuildingInteractionKind.Rest))
                return BuildingInteractionKind.Rest;
            if (HasInteraction(in definition, BuildingInteractionKind.AssignWorker))
                return BuildingInteractionKind.AssignWorker;
            if (HasInteraction(in definition, BuildingInteractionKind.OpenDetails))
                return BuildingInteractionKind.OpenDetails;

            throw new System.InvalidOperationException(
                $"Completed building {definition.Id.Value} has no completed player interaction.");
        }

        private static void RequireInteraction(in BuildingDefinition definition, BuildingInteractionKind kind)
        {
            if (!HasInteraction(in definition, kind))
                throw new System.InvalidOperationException($"Building {definition.Id.Value} is missing interaction {kind}.");
        }

        private static bool HasInteraction(in BuildingDefinition definition, BuildingInteractionKind kind)
        {
            for (var i = 0; i < definition.Interactions.Length; i++)
            {
                if (definition.Interactions[i].Kind != kind)
                    continue;

                return true;
            }

            return false;
        }

        private static string ResolveLabel(BuildingInteractionKind kind)
        {
            switch (kind)
            {
                case BuildingInteractionKind.OpenDetails:
                    return "Open";
                case BuildingInteractionKind.DepositConstructionResources:
                    return "Deposit";
                case BuildingInteractionKind.ContributeBuildWork:
                    return "Build";
                case BuildingInteractionKind.AssignWorker:
                    return "Assign Worker";
                case BuildingInteractionKind.OpenProductionQueue:
                    return "Open Queue";
                case BuildingInteractionKind.SetRecipe:
                    return "Set Recipe";
                case BuildingInteractionKind.ClaimOutput:
                    return "Claim Output";
                case BuildingInteractionKind.AssignBed:
                    return "Assign Bed";
                case BuildingInteractionKind.ToggleEnabled:
                    return "Toggle";
                case BuildingInteractionKind.TriggerRepair:
                    return "Repair";
                case BuildingInteractionKind.Extract:
                    return "Extract";
                case BuildingInteractionKind.Rest:
                    return "Rest";
                case BuildingInteractionKind.StoreItems:
                    return "Open Storage";
                case BuildingInteractionKind.WithdrawItems:
                    return "Withdraw";
                default:
                    return string.Empty;
            }
        }

        private static void PopulateOpenedBuildingAction(
            ref Stage1ContextPanelState state,
            EntityGID target,
            in BuildingDefinition definition)
        {
            ref readonly var intent = ref CW.GetResource<Stage1BuildingOperationOpenIntent>();
            if (!intent.HasIntent || intent.Target != target)
                return;

            state.HasOpenedBuildingAction = true;
            state.OpenedBuildingActionKind = intent.Kind;
            state.OpenedBuildingActionLabel = ResolveLabel(intent.Kind);
            state.OpenedBuildingActionSummary = ResolveOpenedBuildingActionSummary(intent.Kind, in definition);
        }

        private static string ResolveOpenedBuildingActionSummary(
            BuildingInteractionKind kind,
            in BuildingDefinition definition)
        {
            switch (kind)
            {
                case BuildingInteractionKind.OpenDetails:
                    return BuildDetailsSummary(in definition);
                case BuildingInteractionKind.AssignWorker:
                    return $"Worker slots: {definition.NpcProfile.WorkerSlots}";
                case BuildingInteractionKind.OpenProductionQueue:
                case BuildingInteractionKind.SetRecipe:
                case BuildingInteractionKind.ClaimOutput:
                    return BuildProductionSummary(in definition);
                case BuildingInteractionKind.AssignBed:
                case BuildingInteractionKind.Rest:
                    return $"Bed slots: {definition.Operation.WorkerSlots}";
                case BuildingInteractionKind.Extract:
                    return BuildExtractionSummary(in definition);
                case BuildingInteractionKind.StoreItems:
                case BuildingInteractionKind.WithdrawItems:
                    return $"Storage capacity: {definition.Operation.StorageCapacity}";
                case BuildingInteractionKind.ToggleEnabled:
                    return "Toggle operation enabled state.";
                case BuildingInteractionKind.TriggerRepair:
                    return "Repair operation available.";
                default:
                    return BuildDetailsSummary(in definition);
            }
        }

        private static string BuildDetailsSummary(in BuildingDefinition definition)
        {
            return
                $"Category: {ResolveCategoryLabel(definition.Category)}\n" +
                $"Footprint: {definition.FootprintWidth}x{definition.FootprintLength}\n" +
                $"Cost: {FormatResourceAmounts(definition.ConstructionCost)}";
        }

        private static string BuildProductionSummary(in BuildingDefinition definition)
        {
            var builder = new StringBuilder();
            builder.Append("Worker slots: ");
            builder.Append(definition.Operation.WorkerSlots);
            builder.Append("\nRecipes: ");

            for (var i = 0; i < WorkbenchRecipeCatalog.All.Count; i++)
            {
                if (i > 0)
                    builder.Append(", ");

                builder.Append(WorkbenchRecipeCatalog.All[i].Code);
            }

            return builder.ToString();
        }

        private static string BuildExtractionSummary(in BuildingDefinition definition)
        {
            if (!ExtractionRules.TryGetOutputResource(definition.Id, out var output))
                throw new System.InvalidOperationException($"Extraction action requested for non-extraction building {definition.Id.Value}.");

            return
                $"Output: {ResourceCatalog.Get(output).DisplayName}\n" +
                $"Buffer capacity: {definition.Operation.StorageCapacity}\n" +
                $"Worker slots: {definition.Operation.WorkerSlots}";
        }

        private static string FormatResourceAmounts(ResourceAmount[] amounts)
        {
            var builder = new StringBuilder();

            for (var i = 0; i < amounts.Length; i++)
            {
                if (i > 0)
                    builder.Append(", ");

                builder.Append(ResourceCatalog.Get(amounts[i].Id).DisplayName);
                builder.Append(' ');
                builder.Append(amounts[i].Amount);
            }

            return builder.ToString();
        }

        private static string ResolveCategoryLabel(BuildingCategory category)
        {
            switch (category)
            {
                case BuildingCategory.Housing:
                    return "Housing";
                case BuildingCategory.Logistics:
                    return "Logistics";
                case BuildingCategory.Extraction:
                    return "Extraction";
                case BuildingCategory.Production:
                    return "Production";
                case BuildingCategory.Service:
                    return "Service";
                case BuildingCategory.Defense:
                    return "Defense";
                case BuildingCategory.Research:
                    return "Research";
                default:
                    return "Uncategorized";
            }
        }

        private static void CopyProjectedConstructionResources(
            CW.Entity site,
            ref FixedList512Bytes<ConstructionResourceViewEntry> target)
        {
            ref readonly var rows = ref ClientProjection.ReadMulti<ConstructionResourceEntry>(site);
            for (var i = 0; i < rows.Length; i++)
            {
                if (target.Length == target.Capacity)
                    throw new InvalidOperationException(
                        $"{nameof(Stage1ContextPanelState)} cannot hold more than {target.Capacity} construction resource rows.");

                var row = rows[i].Value;
                target.Add(new ConstructionResourceViewEntry(row.Id, row.Required, row.Delivered));
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
