using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Game;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    public sealed class ServerProductionStationProcessingSystem : ISystem
    {
        public void Update()
        {
            var fixedStepSeconds = SW.GetResource<SimulationTime>().FixedStepSeconds;
            var sharedStorage = SettlementSharedResourcesQuery.GetServerEntity();

            foreach (var station in SW.Query<All<FinishedBuildingTag, ConstructionSiteState, ProductionStationOperationState>>().Entities())
            {
                ref readonly var site = ref station.Read<ConstructionSiteState>();
                if (site.Phase != ConstructionPhase.Completed)
                    throw new InvalidOperationException(
                        $"Production station {station.GID.Raw} is marked finished but construction phase is {site.Phase}.");

                ref readonly var snapshot = ref station.Read<ProductionStationOperationState>();
                var enabled = snapshot.Enabled;
                var stationId = snapshot.Station;
                var activeRecipe = snapshot.ActiveRecipe;
                var workerSlotCount = snapshot.WorkerSlotCount;

                if (!enabled)
                {
                    ref var state = ref ReplicationMut.Mut<ProductionStationOperationState>(station);
                    state.BlockedReasonValue = (byte)ProductionStationBlockedReason.None;
                    continue;
                }

                var building = BuildingCatalogData.Get(new BuildingId(site.BuildingId));
                ValidateProductionBuilding(station, in building);

                var recipe = ProductionRecipeCatalog.Get(stationId, activeRecipe);
                ref var state = ref ReplicationMut.Mut<ProductionStationOperationState>(station);
                state.BlockedReasonValue = (byte)ProductionStationBlockedReason.None;

                if (!ProductionStationRules.CanFitOutputs(station, in recipe, building.Operation.StorageCapacity))
                {
                    state.BlockedReasonValue = (byte)ProductionStationBlockedReason.FullOutputBuffer;
                    continue;
                }

                var workerMultiplier = ProductionStationRules.ResolveWorkerMultiplier(
                    workerSlotCount,
                    CountAssignedWorkers(station.GID));
                if (workerMultiplier <= 0)
                {
                    state.BlockedReasonValue = (byte)ProductionStationBlockedReason.NoWorkers;
                    continue;
                }

                if (ProductionStationRules.IsBlockedByFuel(station, sharedStorage, in recipe))
                {
                    state.BlockedReasonValue = (byte)ProductionStationBlockedReason.MissingFuel;
                    continue;
                }

                if (!ProductionStationRules.TryReserveRecipeInputs(station, sharedStorage, in recipe))
                {
                    state.BlockedReasonValue = (byte)ProductionStationBlockedReason.MissingInputs;
                    continue;
                }

                var completed = ProductionStationRules.AdvanceWork(
                    ref state,
                    in recipe,
                    fixedStepSeconds * workerMultiplier);
                if (!completed)
                    continue;

                ProductionStationRules.CommitCompletedRecipe(station, in recipe);
            }
        }

        private static void ValidateProductionBuilding(SW.Entity station, in BuildingDefinition building)
        {
            var required = BuildingCapabilityFlags.ProducesResources
                           | BuildingCapabilityFlags.ConsumesResources
                           | BuildingCapabilityFlags.OpensQueue;
            if ((building.Operation.OperationCapabilities & required) != required)
            {
                throw new InvalidOperationException(
                    $"Production station {station.GID.Raw} building {building.Id.Value} is missing production operation capabilities.");
            }

            if (building.Operation.StorageCapacity == 0)
                throw new InvalidOperationException(
                    $"Production station {station.GID.Raw} building {building.Id.Value} has no output capacity.");
        }

        private static int CountAssignedWorkers(EntityGID building)
        {
            var count = 0;
            foreach (var worker in SW.Query<All<BuildingWorkerAssignmentState>>().Entities())
            {
                ref readonly var assignment = ref worker.Read<BuildingWorkerAssignmentState>();
                if (assignment.IsAssigned && assignment.Building == building)
                    count++;
            }

            return count;
        }
    }
}
