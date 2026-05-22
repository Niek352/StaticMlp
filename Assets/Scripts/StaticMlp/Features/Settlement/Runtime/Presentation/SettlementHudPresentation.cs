using System;
using FFS.Libraries.StaticEcs;
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
                Objective = ToPresentationObjective(flow.Objective),
                ObjectiveHint = ToPresentationHint(flow.Hint),
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

        private static Stage1HudActionPresentation CreateLoadoutPreparationAction(bool enabled)
        {
            return new Stage1HudActionPresentation(
                "Prepare Build",
                enabled,
                enabled ? string.Empty : "Locked until the workbench objective is online.",
                "Opens the loadout preparation screen.");
        }

        private static Stage1HudActionPresentation CreateExpeditionSelectionAction(bool enabled)
        {
            return new Stage1HudActionPresentation(
                "Open Expedition",
                enabled,
                enabled ? string.Empty : "Locked until expedition selection is available.",
                "Opens the expedition selection screen.");
        }

        private static bool TryGetRequiredState(
            out CW.Entity anchor,
            out Stage1FlowViewState flow,
            out CW.Entity resources)
        {
            if (!Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out anchor))
            {
                flow = default;
                resources = default;
                return false;
            }

            flow = ClientProjection.Read<Stage1FlowViewState>(anchor);

            if (!TryReadSharedResources(out resources))
                return false;

            return true;
        }

        private static Stage1ObjectiveKind ToPresentationObjective(Stage1FlowObjective objective)
        {
            switch (objective)
            {
                case Stage1FlowObjective.RepairCamp:
                    return Stage1ObjectiveKind.RepairCamp;
                case Stage1FlowObjective.AssignWorker:
                    return Stage1ObjectiveKind.AssignWorker;
                case Stage1FlowObjective.PlaceStockpile:
                    return Stage1ObjectiveKind.PlaceStockpile;
                case Stage1FlowObjective.PlaceShelter:
                    return Stage1ObjectiveKind.PlaceShelter;
                case Stage1FlowObjective.BringExtractionOnline:
                    return Stage1ObjectiveKind.BringExtractionOnline;
                case Stage1FlowObjective.BringWorkbenchOnline:
                    return Stage1ObjectiveKind.BringWorkbenchOnline;
                case Stage1FlowObjective.PrepareBuild:
                    return Stage1ObjectiveKind.PrepareBuild;
                case Stage1FlowObjective.StartExpedition:
                    return Stage1ObjectiveKind.StartExpedition;
                case Stage1FlowObjective.ClearExpedition:
                    return Stage1ObjectiveKind.ClearExpedition;
                case Stage1FlowObjective.DefendCamp:
                    return Stage1ObjectiveKind.DefendCamp;
                case Stage1FlowObjective.PrepareBoss:
                    return Stage1ObjectiveKind.PrepareBoss;
                case Stage1FlowObjective.StartBossEncounter:
                    return Stage1ObjectiveKind.StartBossEncounter;
                case Stage1FlowObjective.DefeatBoss:
                    return Stage1ObjectiveKind.DefeatBoss;
                case Stage1FlowObjective.VerticalSliceComplete:
                    return Stage1ObjectiveKind.VerticalSliceComplete;
                default:
                    return Stage1ObjectiveKind.None;
            }
        }

        private static string ToPresentationHint(Stage1FlowHint hint)
        {
            switch (hint)
            {
                case Stage1FlowHint.GatherRepairResources:
                    return "Gather the camp resources needed to begin repairs.";
                case Stage1FlowHint.ContinueRepairBuild:
                    return "Resources delivered. Keep building the camp core to finish repairs.";
                case Stage1FlowHint.AssignWorker:
                    return "The camp is repaired. Assign the camp builder to continue the loop.";
                case Stage1FlowHint.PlaceStockpile:
                    return "Place a stockpile so the settlement can hold expanded resources.";
                case Stage1FlowHint.PlaceShelter:
                    return "Place a shelter to establish basic worker service.";
                case Stage1FlowHint.BringExtractionOnline:
                    return "Bring lumber or stone extraction online for steady supply.";
                case Stage1FlowHint.BringWorkbenchOnline:
                    return "Bring the workbench online to prepare the settlement economy.";
                default:
                    return string.Empty;
            }
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

                var row = rows[i].Value;
                target.Add(new SettlementResourceViewEntry(row.Id, row.Amount));
            }
        }
    }
}
