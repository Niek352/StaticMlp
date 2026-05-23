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
        public static BuildingContextPanelState Build(in SettlementContextPanelSession session)
        {
            if (session.Mode != SettlementContextPanelMode.Building)
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
                SecondaryBuildingAction = CreateSecondaryBuildingAction(site.GID, in definition),
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
                return CreateBuildingAction(
                    kind,
                    true,
                    true,
                    string.Empty,
                    site.GID,
                    in definition);
            }

            var resourcesComplete = ConstructionResourcesAccess.IsProjectedComplete(site);
            if (!resourcesComplete)
            {
                RequireInteraction(in definition, BuildingInteractionKind.DepositConstructionResources);
                var enabled = SettlementConstructionRules.CanDepositResources(in state);
                return CreateBuildingAction(
                    BuildingInteractionKind.DepositConstructionResources,
                    true,
                    enabled,
                    enabled ? string.Empty : "Construction is not waiting for resources.",
                    site.GID,
                    in definition);
            }

            RequireInteraction(in definition, BuildingInteractionKind.ContributeBuildWork);
            var canBuild = SettlementConstructionRules.CanProjectedBuild(in state, site);
            return CreateBuildingAction(
                BuildingInteractionKind.ContributeBuildWork,
                true,
                canBuild,
                canBuild ? string.Empty : "Construction is not ready for build work.",
                site.GID,
                in definition);
        }

        private static BuildingAvailableActionPresentation CreateSecondaryBuildingAction(
            EntityGID target,
            in BuildingDefinition definition)
        {
            return CreateBuildingAction(
                BuildingInteractionKind.OpenDetails,
                false,
                true,
                string.Empty,
                target,
                in definition);
        }

        private static BuildingAvailableActionPresentation CreateBuildingAction(
            BuildingInteractionKind kind,
            bool isPrimaryAction,
            bool enabled,
            string disabledReason,
            EntityGID target,
            in BuildingDefinition definition)
        {
            var handler = BuildingInteractionHandlerRegistry.Get(kind);
            return new BuildingAvailableActionPresentation(
                kind,
                GetInteraction(in definition, kind).DisplayName,
                handler.ResolveInputHint(kind, isPrimaryAction),
                handler.ResolveEffectDescription(kind),
                enabled,
                disabledReason,
                target);
        }

        private static BuildingInteractionKind SelectCompletedPrimaryAction(in BuildingDefinition definition)
        {
            var selected = BuildingInteractionKind.None;
            byte selectedPriority = 0;

            for (var i = 0; i < definition.Interactions.Length; i++)
            {
                var interaction = definition.Interactions[i];
                if (!interaction.RequiresCompletedBuilding && interaction.Priority == 0)
                    continue;

                if (selected != BuildingInteractionKind.None && interaction.Priority <= selectedPriority)
                    continue;

                selected = interaction.Kind;
                selectedPriority = interaction.Priority;
            }

            if (selected != BuildingInteractionKind.None)
                return selected;

            throw new System.InvalidOperationException(
                $"Completed building {definition.Id.Value} has no completed player interaction.");
        }

        private static void RequireInteraction(in BuildingDefinition definition, BuildingInteractionKind kind)
        {
            GetInteraction(in definition, kind);
        }

        private static BuildingInteractionDefinition GetInteraction(in BuildingDefinition definition, BuildingInteractionKind kind)
        {
            for (var i = 0; i < definition.Interactions.Length; i++)
            {
                if (definition.Interactions[i].Kind != kind)
                    continue;

                return definition.Interactions[i];
            }

            throw new System.InvalidOperationException($"Building {definition.Id.Value} is missing interaction {kind}.");
        }

        private static void PopulateOpenedBuildingAction(
            ref BuildingContextPanelState state,
            EntityGID target,
            in BuildingDefinition definition)
        {
            ref readonly var intent = ref CW.GetResource<SettlementBuildingOperationOpenIntent>();
            if (!intent.HasIntent || intent.Target != target)
                return;

            state.HasOpenedBuildingAction = true;
            state.OpenedBuildingActionKind = intent.Kind;
            var handler = BuildingInteractionHandlerRegistry.Get(intent.Kind);
            state.OpenedBuildingActionLabel = GetInteraction(in definition, intent.Kind).DisplayName;
            state.OpenedBuildingActionSummary = handler.ResolveSummary(intent.Kind, in definition);
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
