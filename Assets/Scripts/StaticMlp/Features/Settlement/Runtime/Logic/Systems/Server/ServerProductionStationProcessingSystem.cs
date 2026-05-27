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
                if (!snapshot.Enabled)
                    continue;

                var building = BuildingCatalogData.Get(new BuildingId(site.BuildingId));
                ValidateProductionBuilding(station, in building);

                var recipe = ProductionRecipeCatalog.Get(snapshot.Station, snapshot.ActiveRecipe);
                if (!ProductionStationRules.CanFitOutputs(station, in recipe, building.Operation.StorageCapacity))
                    continue;

                var workerMultiplier = ProductionStationRules.ResolveWorkerMultiplier(
                    snapshot.WorkerSlotCount,
                    CountAssignedWorkers(station.GID));
                if (workerMultiplier <= 0)
                    continue;

                if (!ProductionStationRules.TryReserveRecipeInputs(station, sharedStorage, in recipe))
                    continue;

                ref var state = ref ReplicationMut.Mut<ProductionStationOperationState>(station);
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
