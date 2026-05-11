using System;
using UnityEngine;

namespace StaticMlp.Features.Settlement
{
    public static class Stage1SettlementSeedManifest
    {
        private const ushort CAMP_CORE_BUILDING_ID = 1;

        private static readonly Stage1ConstructionSiteSeed[] INITIAL_CONSTRUCTION_SITES =
        {
            new()
            {
                AnchorId = SettlementAnchorCatalog.HomeCampId.Value,
                BuildingId = CAMP_CORE_BUILDING_ID,
                Position = new Vector3(0f, 0f, 16f),
                Rotation = Quaternion.identity,
                StartReadyToBuild = true,
                InitialBuildWork = 0f
            }
        };

        private static readonly ResourceAmount[] STARTING_RESOURCES = CreateStartingResources();
        private static readonly WorkerRoleId[] STARTING_WORKER_ROLES = Array.Empty<WorkerRoleId>();

        public static Stage1SettlementSeed CreateResource()
        {
            return new Stage1SettlementSeed(
                INITIAL_CONSTRUCTION_SITES,
                STARTING_RESOURCES,
                STARTING_WORKER_ROLES);
        }

        private static ResourceAmount[] CreateStartingResources()
        {
            var woodDefinition = ResourceCatalog.Get(ResourceCatalog.WoodId);
            var stoneDefinition = ResourceCatalog.Get(ResourceCatalog.StoneId);

            return new[]
            {
                new ResourceAmount(ResourceCatalog.WoodId, woodDefinition.StartingSettlementAmount),
                new ResourceAmount(ResourceCatalog.StoneId, stoneDefinition.StartingSettlementAmount)
            };
        }
    }
}
