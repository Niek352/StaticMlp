using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.CampFlow;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;
using Unity.Collections;

namespace StaticMlp.Features.Settlement
{
    public static class SettlementHudPresentation
    {
        public static SettlementHudState Build()
        {
            if (!TryGetRequiredState(out var anchor, out var flow, out var resources))
                return default;

            var state = new SettlementHudState
            {
                AnchorId = flow.Anchor,
                ObjectiveDisplayName = flow.ObjectiveDisplayName,
                HintDisplayName = flow.HintDisplayName,
                SettlementStage = flow.Stage,
                CanOpenLoadoutPreparation = flow.CanOpenLoadoutPreparation,
                CanOpenExpeditionSelection = flow.CanOpenExpeditionSelection,
                LoadoutPreparationAction = CreateLoadoutPreparationAction(flow.CanOpenLoadoutPreparation),
                ExpeditionSelectionAction = CreateExpeditionSelectionAction(flow.CanOpenExpeditionSelection),
            };
            CopyProjectedResources(resources, ref state.Resources);

            if (anchor.Has<Projected<SettlementWorkerSummary>>())
            {
                ref readonly var summary = ref ClientProjection.Read<SettlementWorkerSummary>(anchor);
                state.TotalWorkers = summary.TotalWorkers;
                state.AssignedWorkers = summary.AssignedWorkers;
                state.WorkerBlockingReason = summary.BlockingReason;
            }

            return state;
        }

        private static HudActionPresentation CreateLoadoutPreparationAction(bool enabled)
        {
            return new HudActionPresentation(
                "Prepare Build",
                enabled,
                enabled ? string.Empty : "Locked until the workbench objective is online.",
                "Opens the loadout preparation screen.");
        }

        private static HudActionPresentation CreateExpeditionSelectionAction(bool enabled)
        {
            return new HudActionPresentation(
                "Open Expedition",
                enabled,
                enabled ? string.Empty : "Locked until expedition selection is available.",
                "Opens the expedition selection screen.");
        }

        private static bool TryGetRequiredState(
            out CW.Entity anchor,
            out CampFlowViewState flow,
            out CW.Entity resources)
        {
            if (!CampFlowProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out anchor))
            {
                flow = default;
                resources = default;
                return false;
            }

            flow = ClientProjection.Read<CampFlowViewState>(anchor);

            if (!TryReadSharedResources(out resources))
                return false;

            return true;
        }

        private static bool TryReadSharedResources(out CW.Entity resources)
        {
            foreach (var entity in CW.Query<All<SettlementResourceStorageTag, SettlementSharedResources>>().Entities())
            {
                resources = entity;
                return true;
            }

            resources = default;
            return false;
        }

        private static void CopyProjectedResources(
            CW.Entity resources,
            ref FixedList512Bytes<SettlementResourceViewEntry> target)
        {
            ref readonly var rows = ref ClientProjection.ReadMulti<SettlementStoredResource>(resources);
            for (var i = 0; i < rows.Length; i++)
            {
                if (target.Length == target.Capacity)
                    throw new InvalidOperationException(
                        $"{nameof(SettlementHudState)} cannot hold more than {target.Capacity} settlement resource rows.");

                ref var row = ref rows[i].Value;
                target.Add(new SettlementResourceViewEntry(row.Id, row.Amount));
            }
        }
    }
}
