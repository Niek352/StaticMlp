using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;
using Unity.Collections;

namespace StaticMlp.Features.Settlement
{
    public static class BuildingPanelPresentation
    {
        public static BuildingPanelState Build(in BuildingPanelSession session)
        {
            if (!session.IsOpen)
                return default;

            var target = GetBuildingEntity(session.Target);
            ref readonly var site = ref ClientProjection.Read<ConstructionSiteState>(target);
            var definition = BuildingCatalogData.Get(new BuildingId(site.BuildingId));
            var kind = session.Kind;

            var next = new BuildingPanelState
            {
                IsOpen = true,
                Target = session.Target,
                Kind = kind,
                Title = definition.DisplayName
            };

            switch (kind)
            {
                case BuildingPanelKind.ConstructionSitePanel:
                    next.Construction = BuildConstruction(target, in site, in definition);
                    next.PrimaryAction = CreateDepositAction(target, in site);
                    next.SecondaryAction = CreateBuildAction(target, in site);
                    return next;
                case BuildingPanelKind.StockpilePanel:
                    next.Stockpile = BuildStockpile(target, in definition);
                    next.PrimaryAction = CreateDepositCarriedResourcesToStockpileAction(in next.Stockpile);
                    return next;
                case BuildingPanelKind.ExtractionPanel:
                    next.Extraction = BuildExtraction(target, in definition);
                    next.PrimaryAction = CreateWorkerAction(in next.Extraction);
                    next.SecondaryAction = CreateCollectExtractionAction(in next.Extraction);
                    return next;
                case BuildingPanelKind.WorkbenchPanel:
                    next.Workbench = BuildWorkbench(target, in definition);
                    next.PrimaryAction = CreateWorkerAction(in next.Workbench);
                    return next;
                case BuildingPanelKind.ShelterPanel:
                    next.Shelter = BuildShelter(target, in definition);
                    return next;
                case BuildingPanelKind.CampCorePanel:
                    next.CampCore = BuildCampCore(target, in site, in definition);
                    return next;
                default:
                    throw new InvalidOperationException($"Unsupported building panel kind {kind}.");
            }
        }

        private static ConstructionPanelState BuildConstruction(
            CW.Entity target,
            in ConstructionSiteState site,
            in BuildingDefinition definition)
        {
            ref readonly var progress = ref ClientProjection.Read<ConstructionProgress>(target);
            var state = new ConstructionPanelState
            {
                Target = target.GID,
                DisplayName = definition.DisplayName,
                Phase = site.Phase,
                Progress01 = progress.Normalized
            };
            CopyProjectedConstructionResources(target, ref state.Resources);
            return state;
        }

        private static StockpilePanelState BuildStockpile(CW.Entity target, in BuildingDefinition definition)
        {
            var storageEntity = SettlementSharedResourcesQuery.GetClientEntity();
            ref readonly var storage = ref ClientProjection.Read<SettlementSharedResources>(storageEntity);
            ref readonly var stockpile = ref ClientProjection.Read<StockpileOperationState>(target);
            var state = new StockpilePanelState
            {
                Target = target.GID,
                DisplayName = definition.DisplayName,
                UsedCapacity = SettlementSharedResourcesAccess.TotalProjectedUsed(storageEntity),
                Capacity = storage.Capacity,
                ContributedCapacity = stockpile.ContributedCapacity
            };
            CopyProjectedStoredResources(storageEntity, ref state.Resources);
            return state;
        }

        private static ExtractionPanelState BuildExtraction(CW.Entity target, in BuildingDefinition definition)
        {
            ref readonly var extraction = ref ClientProjection.Read<ExtractionOperationState>(target);
            ref readonly var anchorRef = ref ClientProjection.Read<SettlementAnchorRef>(target);
            var state = new ExtractionPanelState
            {
                Target = target.GID,
                AnchorId = anchorRef.Anchor,
                DisplayName = definition.DisplayName,
                OutputResource = extraction.OutputResource,
                BufferAmount = extraction.OutputBufferAmount,
                BufferCapacity = extraction.OutputBufferCapacity,
                WorkerSlotCount = extraction.WorkerSlotCount
            };
            PopulateWorkerSlots(ref state);
            return state;
        }

        private static WorkbenchPanelState BuildWorkbench(CW.Entity target, in BuildingDefinition definition)
        {
            ref readonly var workbench = ref ClientProjection.Read<WorkbenchOperationState>(target);
            ref readonly var anchorRef = ref ClientProjection.Read<SettlementAnchorRef>(target);
            var recipe = WorkbenchRecipeCatalog.Get(workbench.ActiveRecipe);
            var state = new WorkbenchPanelState
            {
                Target = target.GID,
                AnchorId = anchorRef.Anchor,
                DisplayName = definition.DisplayName,
                RecipeName = recipe.Code,
                WorkDone = workbench.WorkDone,
                WorkRequired = recipe.WorkRequired,
                WorkerSlotCount = workbench.WorkerSlotCount
            };
            CopyAmounts(recipe.Inputs, ref state.Inputs);
            CopyAmounts(recipe.Outputs, ref state.Outputs);
            PopulateWorkerSlots(ref state);
            return state;
        }

        private static ShelterPanelState BuildShelter(CW.Entity target, in BuildingDefinition definition)
        {
            ref readonly var shelter = ref ClientProjection.Read<BedrollShelterState>(target);
            return new ShelterPanelState
            {
                Target = target.GID,
                DisplayName = definition.DisplayName,
                SlotCount = shelter.SlotCount,
                FreeSlots = shelter.SlotCount,
                Enabled = shelter.Enabled
            };
        }

        private static CampCorePanelState BuildCampCore(
            CW.Entity target,
            in ConstructionSiteState site,
            in BuildingDefinition definition)
        {
            ref readonly var progress = ref ClientProjection.Read<ConstructionProgress>(target);
            return new CampCorePanelState
            {
                Target = target.GID,
                DisplayName = definition.DisplayName,
                Phase = site.Phase,
                Progress01 = progress.Normalized
            };
        }

        private static BuildingPanelAction CreateDepositAction(CW.Entity target, in ConstructionSiteState site)
        {
            var enabled = !ConstructionResourcesAccess.IsProjectedComplete(target)
                          && SettlementConstructionRules.CanDepositResources(in site);
            return new BuildingPanelAction(
                BuildingPanelActionKind.DepositConstructionResources,
                "Deposit",
                enabled,
                enabled ? string.Empty : "Resources are complete or construction is not accepting deposits.",
                target.GID);
        }

        private static BuildingPanelAction CreateBuildAction(CW.Entity target, in ConstructionSiteState site)
        {
            var enabled = SettlementConstructionRules.CanProjectedBuild(in site, target);
            return new BuildingPanelAction(
                BuildingPanelActionKind.ContributeBuildWork,
                "Build",
                enabled,
                enabled ? string.Empty : "Construction is not ready for build work.",
                target.GID);
        }

        private static BuildingPanelAction CreateWorkerAction(in ExtractionPanelState state)
        {
            if (TryFindAssignedSlot(in state, out var assignedSlot))
            {
                return new BuildingPanelAction(
                    BuildingPanelActionKind.UnassignWorker,
                    "Unassign Worker",
                    enabled: true,
                    disabledReason: string.Empty,
                    state.Target,
                    assignedSlot.Worker,
                    assignedSlot.SlotIndex);
            }

            var hasFreeSlot = TryFindFreeSlot(in state, out var freeSlot);
            var hasWorker = TryFindAssignableWorker(state.AnchorId, out var worker);
            var enabled = hasFreeSlot && hasWorker;
            return new BuildingPanelAction(
                BuildingPanelActionKind.AssignWorker,
                "Assign Worker",
                enabled,
                enabled ? string.Empty : ResolveAssignDisabledReason(hasFreeSlot, hasWorker),
                state.Target,
                worker,
                freeSlot);
        }

        private static BuildingPanelAction CreateWorkerAction(in WorkbenchPanelState state)
        {
            if (TryFindAssignedSlot(in state, out var assignedSlot))
            {
                return new BuildingPanelAction(
                    BuildingPanelActionKind.UnassignWorker,
                    "Unassign Worker",
                    enabled: true,
                    disabledReason: string.Empty,
                    state.Target,
                    assignedSlot.Worker,
                    assignedSlot.SlotIndex);
            }

            var hasFreeSlot = TryFindFreeSlot(in state, out var freeSlot);
            var hasWorker = TryFindAssignableWorker(state.AnchorId, out var worker);
            var enabled = hasFreeSlot && hasWorker;
            return new BuildingPanelAction(
                BuildingPanelActionKind.AssignWorker,
                "Assign Worker",
                enabled,
                enabled ? string.Empty : ResolveAssignDisabledReason(hasFreeSlot, hasWorker),
                state.Target,
                worker,
                freeSlot);
        }

        private static BuildingPanelAction CreateCollectExtractionAction(in ExtractionPanelState state)
        {
            var enabled = state.BufferAmount > 0;
            return new BuildingPanelAction(
                BuildingPanelActionKind.CollectExtractionOutput,
                "Collect Output",
                enabled,
                enabled ? string.Empty : "Output buffer is empty.",
                state.Target,
                resource: state.OutputResource,
                amount: state.BufferAmount);
        }

        private static BuildingPanelAction CreateDepositCarriedResourcesToStockpileAction(in StockpilePanelState state)
        {
            var enabled = StockpileRules.HasAvailableCapacity(state.Capacity, state.UsedCapacity);
            return new BuildingPanelAction(
                BuildingPanelActionKind.DepositCarriedResourcesToStockpile,
                "Store Items",
                enabled,
                enabled ? string.Empty : "Stockpile storage is full.",
                state.Target);
        }

        private static string ResolveAssignDisabledReason(bool hasFreeSlot, bool hasWorker)
        {
            if (!hasFreeSlot)
                return "All worker slots are occupied.";

            if (!hasWorker)
                return "No available worker belongs to this settlement.";

            return string.Empty;
        }

        private static void PopulateWorkerSlots(ref ExtractionPanelState state)
        {
            for (byte i = 0; i < state.WorkerSlotCount; i++)
            {
                if (state.WorkerSlots.Length == state.WorkerSlots.Capacity)
                    throw new InvalidOperationException(
                        $"{nameof(ExtractionPanelState)} cannot hold more than {state.WorkerSlots.Capacity} worker slots.");

                state.WorkerSlots.Add(new BuildingWorkerSlot(i, default, assigned: false));
            }

            foreach (var worker in CW.Query<All<SettlementWorkerTag, BuildingWorkerAssignmentState>>().Entities())
            {
                ref readonly var assignment = ref ClientProjection.Read<BuildingWorkerAssignmentState>(worker);
                if (!assignment.IsAssigned || assignment.Building != state.Target)
                    continue;

                if (assignment.SlotIndex >= state.WorkerSlots.Length)
                    throw new InvalidOperationException(
                        $"Worker {worker.GID.Raw} is assigned to slot {assignment.SlotIndex}, but building {state.Target.Raw} exposes {state.WorkerSlots.Length} slots.");

                state.WorkerSlots[assignment.SlotIndex] = new BuildingWorkerSlot(
                    assignment.SlotIndex,
                    worker.GID,
                    assigned: true);
                state.AssignedWorkerCount++;
            }
        }

        private static void PopulateWorkerSlots(ref WorkbenchPanelState state)
        {
            for (byte i = 0; i < state.WorkerSlotCount; i++)
            {
                if (state.WorkerSlots.Length == state.WorkerSlots.Capacity)
                    throw new InvalidOperationException(
                        $"{nameof(WorkbenchPanelState)} cannot hold more than {state.WorkerSlots.Capacity} worker slots.");

                state.WorkerSlots.Add(new BuildingWorkerSlot(i, default, assigned: false));
            }

            foreach (var worker in CW.Query<All<SettlementWorkerTag, BuildingWorkerAssignmentState>>().Entities())
            {
                ref readonly var assignment = ref ClientProjection.Read<BuildingWorkerAssignmentState>(worker);
                if (!assignment.IsAssigned || assignment.Building != state.Target)
                    continue;

                if (assignment.SlotIndex >= state.WorkerSlots.Length)
                    throw new InvalidOperationException(
                        $"Worker {worker.GID.Raw} is assigned to slot {assignment.SlotIndex}, but building {state.Target.Raw} exposes {state.WorkerSlots.Length} slots.");

                state.WorkerSlots[assignment.SlotIndex] = new BuildingWorkerSlot(
                    assignment.SlotIndex,
                    worker.GID,
                    assigned: true);
                state.AssignedWorkerCount++;
            }
        }

        private static bool TryFindAssignedSlot(in ExtractionPanelState state, out BuildingWorkerSlot slot)
        {
            for (var i = 0; i < state.WorkerSlots.Length; i++)
            {
                var candidate = state.WorkerSlots[i];
                if (!candidate.Assigned)
                    continue;

                slot = candidate;
                return true;
            }

            slot = default;
            return false;
        }

        private static bool TryFindFreeSlot(in ExtractionPanelState state, out byte slotIndex)
        {
            for (var i = 0; i < state.WorkerSlots.Length; i++)
            {
                var slot = state.WorkerSlots[i];
                if (slot.Assigned)
                    continue;

                slotIndex = slot.SlotIndex;
                return true;
            }

            slotIndex = 0;
            return false;
        }

        private static bool TryFindAssignedSlot(in WorkbenchPanelState state, out BuildingWorkerSlot slot)
        {
            for (var i = 0; i < state.WorkerSlots.Length; i++)
            {
                var candidate = state.WorkerSlots[i];
                if (!candidate.Assigned)
                    continue;

                slot = candidate;
                return true;
            }

            slot = default;
            return false;
        }

        private static bool TryFindFreeSlot(in WorkbenchPanelState state, out byte slotIndex)
        {
            for (var i = 0; i < state.WorkerSlots.Length; i++)
            {
                var slot = state.WorkerSlots[i];
                if (slot.Assigned)
                    continue;

                slotIndex = slot.SlotIndex;
                return true;
            }

            slotIndex = 0;
            return false;
        }

        private static bool TryFindAssignableWorker(SettlementAnchorId anchorId, out EntityGID workerId)
        {
            foreach (var worker in CW.Query<All<SettlementWorkerTag, SettlementWorkerIdentity, BuildingWorkerAssignmentState>>().Entities())
            {
                ref readonly var identity = ref worker.Read<SettlementWorkerIdentity>();
                if (identity.HomeAnchorId != anchorId.Value)
                    continue;

                ref readonly var assignment = ref ClientProjection.Read<BuildingWorkerAssignmentState>(worker);
                if (assignment.IsAssigned)
                    continue;

                workerId = worker.GID;
                return true;
            }

            workerId = default;
            return false;
        }

        private static byte CountAssignedWorkers(EntityGID building)
        {
            byte count = 0;
            foreach (var worker in CW.Query<All<SettlementWorkerTag, BuildingWorkerAssignmentState>>().Entities())
            {
                ref readonly var assignment = ref ClientProjection.Read<BuildingWorkerAssignmentState>(worker);
                if (assignment.IsAssigned && assignment.Building == building)
                    count++;
            }

            return count;
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
                        $"{nameof(ConstructionPanelState)} cannot hold more than {target.Capacity} construction resource rows.");

                ref var row = ref rows[i].Value;
                target.Add(new ConstructionResourceViewEntry(row.Id, row.Required, row.Delivered));
            }
        }

        private static void CopyProjectedStoredResources(
            CW.Entity storage,
            ref FixedList512Bytes<SettlementResourceViewEntry> target)
        {
            ref readonly var rows = ref ClientProjection.ReadMulti<SettlementStoredResource>(storage);
            for (var i = 0; i < rows.Length; i++)
            {
                if (target.Length == target.Capacity)
                    throw new InvalidOperationException(
                        $"{nameof(StockpilePanelState)} cannot hold more than {target.Capacity} stored resource rows.");

                ref var row = ref rows[i].Value;
                target.Add(new SettlementResourceViewEntry(row.Id, row.Amount));
            }
        }

        private static void CopyAmounts(ResourceAmount[] source, ref FixedList128Bytes<ResourceAmount> target)
        {
            for (var i = 0; i < source.Length; i++)
            {
                if (target.Length == target.Capacity)
                    throw new InvalidOperationException(
                        $"{nameof(WorkbenchPanelState)} cannot hold more than {target.Capacity} resource rows.");

                target.Add(source[i]);
            }
        }

        private static CW.Entity GetBuildingEntity(EntityGID gid)
        {
            if (!gid.TryUnpack<ClientCoreWT>(out var target))
                throw new InvalidOperationException($"Building panel target {gid.Raw} is not a client entity.");

            if (!target.Has<ConstructionSiteState>())
                throw new InvalidOperationException($"Building panel target {gid.Raw} is not a construction/building entity.");

            return target;
        }
    }
}
