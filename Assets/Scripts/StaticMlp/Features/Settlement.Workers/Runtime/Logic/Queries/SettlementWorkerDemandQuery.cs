using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement.Workers
{
    public static class SettlementWorkerDemandQuery
    {
        public static bool TryFindBestDemand(
            SW.Entity worker,
            WorkerJobFlags allowedJobs,
            out SettlementWorkerDemand demand)
        {
            if ((allowedJobs & WorkerJobFlags.DeliverConstructionResources) != 0
                && TryFindConstructionDeliveryDemand(worker, out demand))
            {
                return true;
            }

            if ((allowedJobs & WorkerJobFlags.BuildConstruction) != 0
                && TryFindConstructionBuildDemand(worker, out demand))
            {
                return true;
            }

            if ((allowedJobs & WorkerJobFlags.HaulResources) != 0
                && TryFindHaulDemand(out demand))
            {
                return true;
            }

            if ((allowedJobs & WorkerJobFlags.GatherResources) != 0
                && TryFindGatherDemand(out demand))
            {
                return true;
            }

            if ((allowedJobs & WorkerJobFlags.ProcessRecipe) != 0
                && TryFindProcessDemand(out demand))
            {
                return true;
            }

            demand = default;
            return false;
        }

        public static SettlementWorkerBlockingReason GetNoDemandReason(WorkerJobFlags allowedJobs)
        {
            if ((allowedJobs & (WorkerJobFlags.DeliverConstructionResources | WorkerJobFlags.BuildConstruction)) != 0)
            {
                return HasAnyResourceWaitingSite()
                    ? SettlementWorkerBlockingReason.MissingResources
                    : SettlementWorkerBlockingReason.NoConstructionDemand;
            }

            if ((allowedJobs & WorkerJobFlags.HaulResources) != 0)
                return SettlementWorkerBlockingReason.NoHaulDemand;

            if ((allowedJobs & WorkerJobFlags.GatherResources) != 0)
                return SettlementWorkerBlockingReason.NoGatherDemand;

            if ((allowedJobs & WorkerJobFlags.ProcessRecipe) != 0)
                return SettlementWorkerBlockingReason.NoProcessDemand;

            return SettlementWorkerBlockingReason.NoEligibleDemand;
        }

        public static bool TryFindConstructionDeliveryDemand(SW.Entity worker, out SettlementWorkerDemand demand)
        {
            ref readonly var workerState = ref worker.Read<CharacterNetState>();
            var storageEntity = SettlementSharedResourcesQuery.GetServerEntity();
            var bestDistanceSq = float.MaxValue;
            demand = default;

            foreach (var site in SW.Query<All<ConstructionSiteTag, ConstructionSiteState, ConstructionResources, ConstructionTransform>>().Entities())
            {
                ref readonly var siteState = ref site.Read<ConstructionSiteState>();
                if (!SettlementConstructionRules.TryPlanResourceDeposit(
                        in siteState,
                        site,
                        storageEntity,
                        ConstructionResourcesAccess.GetRemainingResources(site),
                        out var acceptedResources)
                    || acceptedResources.Length == 0)
                {
                    continue;
                }

                ref readonly var transform = ref site.Read<ConstructionTransform>();
                var distanceSq = (transform.Position - workerState.Position).sqrMagnitude;
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                demand = new SettlementWorkerDemand(
                    SettlementWorkerDemand.DemandKind.ConstructionDelivery,
                    AiTaskType.DeliveryResourceToBuilding,
                    site.GID,
                    acceptedResources[0].Id,
                    acceptedResources[0].Amount);
            }

            return demand.Target.TryUnpack<ServerWT>(out _);
        }

        public static bool TryFindConstructionBuildDemand(SW.Entity worker, out SettlementWorkerDemand demand)
        {
            ref readonly var workerState = ref worker.Read<CharacterNetState>();
            var bestDistanceSq = float.MaxValue;
            demand = default;

            foreach (var site in SW.Query<All<ConstructionSiteTag, ConstructionSiteState, ConstructionResources, ConstructionTransform, ConstructionProgress>>().Entities())
            {
                ref readonly var siteState = ref site.Read<ConstructionSiteState>();
                if (!ConstructionRules.CanBuild(site, in siteState))
                    continue;

                ref readonly var transform = ref site.Read<ConstructionTransform>();
                var distanceSq = (transform.Position - workerState.Position).sqrMagnitude;
                if (distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                demand = new SettlementWorkerDemand(
                    SettlementWorkerDemand.DemandKind.ConstructionBuild,
                    AiTaskType.BuildConstruction,
                    site.GID,
                    default,
                    0);
            }

            return demand.Target.TryUnpack<ServerWT>(out _);
        }

        public static bool TryFindGatherDemand(out SettlementWorkerDemand demand)
        {
            foreach (var station in SW.Query<All<ProductionStationOperationState>>().Entities())
            {
                ref readonly var state = ref station.Read<ProductionStationOperationState>();
                if (!IsEnabledWorkbench(in state))
                    continue;

                var recipe = ProductionRecipeCatalog.Get(state.Station, state.ActiveRecipe);
                for (var i = 0; i < recipe.Inputs.Length; i++)
                {
                    var input = recipe.Inputs[i];
                    ref readonly var resource = ref ResourceCatalog.Get(input.Id);
                    if (resource.Family != ResourceFamily.Raw)
                        continue;

                    var missing = input.Amount - ProductionStationResourceAccess.GetInput(station, input.Id);
                    if (missing <= 0)
                        continue;

                    demand = new SettlementWorkerDemand(
                        SettlementWorkerDemand.DemandKind.Gather,
                        AiTaskType.GatherResources,
                        station.GID,
                        input.Id,
                        missing);
                    return true;
                }
            }

            demand = default;
            return false;
        }

        public static bool TryFindHaulDemand(out SettlementWorkerDemand demand)
        {
            if (!HasEnabledStockpile())
            {
                demand = default;
                return false;
            }

            if (TryFindExtractionHaulDemand(out demand))
                return true;

            foreach (var station in SW.Query<All<ProductionStationOperationState>>().Entities())
            {
                ref readonly var state = ref station.Read<ProductionStationOperationState>();
                if (!IsEnabledWorkbench(in state))
                    continue;

                var recipe = ProductionRecipeCatalog.Get(state.Station, state.ActiveRecipe);
                for (var i = 0; i < recipe.Outputs.Length; i++)
                {
                    var output = recipe.Outputs[i];
                    var amount = ProductionStationResourceAccess.GetOutput(station, output.Id);
                    if (amount <= 0)
                        continue;

                    demand = new SettlementWorkerDemand(
                        SettlementWorkerDemand.DemandKind.Haul,
                        AiTaskType.HaulResources,
                        station.GID,
                        output.Id,
                        amount);
                    return true;
                }
            }

            demand = default;
            return false;
        }

        private static bool TryFindExtractionHaulDemand(out SettlementWorkerDemand demand)
        {
            var storageEntity = SettlementSharedResourcesQuery.GetServerEntity();
            ref readonly var storage = ref storageEntity.Read<SettlementSharedResources>();
            var totalUsed = SettlementSharedResourcesAccess.TotalUsed(storageEntity);
            var remainingCapacity = storage.Capacity - totalUsed;
            if (remainingCapacity <= 0)
            {
                demand = default;
                return false;
            }

            foreach (var building in SW.Query<All<FinishedBuildingTag, ExtractionOperationState, ConstructionTransform>>().Entities())
            {
                ref readonly var state = ref building.Read<ExtractionOperationState>();
                if (!ExtractionRules.HasOutput(in state))
                    continue;

                var acceptedAmount = StockpileRules.ClampToCapacity(storage.Capacity, totalUsed, state.OutputBufferAmount);
                if (acceptedAmount <= 0)
                    continue;

                demand = new SettlementWorkerDemand(
                    SettlementWorkerDemand.DemandKind.Haul,
                    AiTaskType.HaulResources,
                    building.GID,
                    state.OutputResource,
                    acceptedAmount);
                return true;
            }

            demand = default;
            return false;
        }

        public static bool TryFindProcessDemand(out SettlementWorkerDemand demand)
        {
            foreach (var station in SW.Query<All<ProductionStationOperationState>>().Entities())
            {
                ref readonly var state = ref station.Read<ProductionStationOperationState>();
                if (!IsEnabledWorkbench(in state) || state.WorkerSlotCount == 0)
                    continue;

                var recipe = ProductionRecipeCatalog.Get(state.Station, state.ActiveRecipe);
                if (state.WorkDone >= recipe.WorkRequired || !HasRecipeInputs(station, in recipe))
                    continue;

                var output = recipe.Outputs[0];
                demand = new SettlementWorkerDemand(
                    SettlementWorkerDemand.DemandKind.Process,
                    AiTaskType.ProcessRecipe,
                    station.GID,
                    output.Id,
                    output.Amount);
                return true;
            }

            demand = default;
            return false;
        }

        private static bool HasRecipeInputs(SW.Entity station, in ProductionRecipeDefinition recipe)
        {
            for (var i = 0; i < recipe.Inputs.Length; i++)
            {
                var input = recipe.Inputs[i];
                if (ProductionStationResourceAccess.GetInput(station, input.Id) < input.Amount)
                    return false;
            }

            return true;
        }

        private static bool IsEnabledWorkbench(in ProductionStationOperationState state)
        {
            return state.Enabled && state.Station == ProductionStationIds.Workbench;
        }

        private static bool HasEnabledStockpile()
        {
            foreach (var stockpile in SW.Query<All<StockpileOperationState>>().Entities())
            {
                ref readonly var state = ref stockpile.Read<StockpileOperationState>();
                if (state.Enabled && state.ContributedCapacity > 0)
                    return true;
            }

            return false;
        }

        private static bool HasAnyResourceWaitingSite()
        {
            foreach (var site in SW.Query<All<ConstructionSiteTag, ConstructionSiteState>>().Entities())
            {
                ref readonly var siteState = ref site.Read<ConstructionSiteState>();
                if (SettlementConstructionRules.CanDepositResources(in siteState))
                    return true;
            }

            return false;
        }
    }
}
