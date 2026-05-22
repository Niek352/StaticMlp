using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;
using Unity.Collections;

namespace StaticMlp.Features.Settlement
{
    public static class BuildingContextPanelPresentation
    {
        public static BuildingContextPanelState Build(in Stage1ContextPanelSession session)
        {
            if (session.Mode != Stage1ContextPanelMode.Building)
                return default;

            if (!TryGetConstructionSite(session.FocusedSite, out var site))
                return default;

            ref readonly var state = ref ClientProjection.Read<ConstructionSiteState>(site);
            ref readonly var progress = ref ClientProjection.Read<ConstructionProgress>(site);
            var definition = BuildingCatalogData.Get(new BuildingId(state.BuildingId));

            var next = new BuildingContextPanelState
            {
                AnchorId = session.WorkerAnchorId,
                FocusedSite = session.FocusedSite,
                HasFocusedSite = true,
                BuildingDisplayName = definition.DisplayName,
                ConstructionPhase = state.Phase,
                Progress01 = progress.Normalized,
                PrimaryBuildingAction = CreatePrimaryBuildingAction(site, in state, in definition),
                SecondaryBuildingAction = CreateSecondaryBuildingAction(site.GID),
            };
            CopyProjectedConstructionResources(site, ref next.ConstructionResources);
            PopulateOpenedBuildingAction(ref next, site.GID, in definition);
            next.CanDepositResources = next.PrimaryBuildingAction.Kind == BuildingInteractionKind.DepositConstructionResources
                                       && next.PrimaryBuildingAction.Enabled;
            next.CanBuild = next.PrimaryBuildingAction.Kind == BuildingInteractionKind.ContributeBuildWork
                            && next.PrimaryBuildingAction.Enabled;

            return next;
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
                    BuildingActionPresentationCatalog.ResolveLabel(kind),
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
                    BuildingActionPresentationCatalog.ResolveLabel(BuildingInteractionKind.DepositConstructionResources),
                    enabled,
                    enabled ? string.Empty : "Construction is not waiting for resources.",
                    target: site.GID);
            }

            RequireInteraction(in definition, BuildingInteractionKind.ContributeBuildWork);
            var canBuild = SettlementConstructionRules.CanProjectedBuild(in state, site);
            return new BuildingAvailableActionPresentation(
                BuildingInteractionKind.ContributeBuildWork,
                BuildingActionPresentationCatalog.ResolveLabel(BuildingInteractionKind.ContributeBuildWork),
                canBuild,
                canBuild ? string.Empty : "Construction is not ready for build work.",
                target: site.GID);
        }

        private static BuildingAvailableActionPresentation CreateSecondaryBuildingAction(EntityGID target)
        {
            return new BuildingAvailableActionPresentation(
                BuildingInteractionKind.OpenDetails,
                BuildingActionPresentationCatalog.ResolveLabel(BuildingInteractionKind.OpenDetails),
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

        private static void PopulateOpenedBuildingAction(
            ref BuildingContextPanelState state,
            EntityGID target,
            in BuildingDefinition definition)
        {
            ref readonly var intent = ref CW.GetResource<Stage1BuildingOperationOpenIntent>();
            if (!intent.HasIntent || intent.Target != target)
                return;

            state.HasOpenedBuildingAction = true;
            state.OpenedBuildingActionKind = intent.Kind;
            state.OpenedBuildingActionLabel = BuildingActionPresentationCatalog.ResolveLabel(intent.Kind);
            state.OpenedBuildingActionSummary = BuildingActionPresentationCatalog.ResolveSummary(intent.Kind, in definition);
        }

        private static void CopyProjectedConstructionResources(
            CW.Entity site,
            ref FixedList512Bytes<ConstructionResourceViewEntry> target)
        {
            ref readonly var rows = ref ClientProjection.ReadMulti<ConstructionResourceEntry>(site);
            for (var i = 0; i < rows.Length; i++)
            {
                if (target.Length == target.Capacity)
                    throw new System.InvalidOperationException(
                        $"{nameof(BuildingContextPanelState)} cannot hold more than {target.Capacity} construction resource rows.");

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
