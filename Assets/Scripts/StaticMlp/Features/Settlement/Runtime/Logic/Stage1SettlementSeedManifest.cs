using System;
using StaticMlp.Features.BuildingCatalog;
using UnityEngine;

namespace StaticMlp.Features.Settlement
{
    public static class Stage1SettlementSeedManifest
    {
        private static readonly Stage1ConstructionSiteSeed[] INITIAL_CONSTRUCTION_SITES =
        {
            new()
            {
                AnchorId = SettlementAnchorCatalog.HomeCampId.Value,
                BuildingId = BuildingCatalogData.CampCoreId.Value,
                Position = new Vector3(0f, 0f, 16f),
                Rotation = Quaternion.identity,
                StartReadyToBuild = true,
                InitialBuildWork = 0f
            }
        };

        private static readonly ResourceAmount[] STARTING_RESOURCES = CreateStartingResources();
        private static readonly Stage1SettlementWorkerSeed[] INITIAL_WORKERS = CreateInitialWorkers();

        public static Stage1SettlementSeed CreateResource()
        {
            return new Stage1SettlementSeed(
                INITIAL_CONSTRUCTION_SITES,
                STARTING_RESOURCES,
                INITIAL_WORKERS);
        }

        private static ResourceAmount[] CreateStartingResources()
        {
            var resources = ResourceCatalog.All;
            var startingResources = new ResourceAmount[resources.Count];

            for (var i = 0; i < resources.Count; i++)
            {
                var definition = resources[i];
                if (!definition.IsSettlementStored)
                    throw new InvalidOperationException($"Stage1 seed cannot initialize non-stored settlement resource id {definition.Id.Value}.");

                startingResources[i] = new ResourceAmount(definition.Id, definition.StartingSettlementAmount);
            }

            return startingResources;
        }

        private static Stage1SettlementWorkerSeed[] CreateInitialWorkers()
        {
            return new[]
            {
                new Stage1SettlementWorkerSeed
                {
                    AnchorId = SettlementAnchorCatalog.HomeCampId.Value,
                    RoleId = WorkerRoleCatalog.CampBuilderId.Value,
                    Position = new Vector3(-2f, 0f, 14f),
                    Rotation = Quaternion.identity
                }
            };
        }
    }
}
