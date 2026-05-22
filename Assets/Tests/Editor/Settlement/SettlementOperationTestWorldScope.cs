using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Tests.Settlement
{
    public sealed class SettlementOperationTestWorldScope : IDisposable
    {
        public SettlementOperationTestWorldScope()
        {
            if (SW.Status != WorldStatus.NotCreated)
                SW.Destroy();

            SW.Create(WorldConfig.Default());
            SW.Types().RegisterAll(
                typeof(ServerWT).Assembly,
                typeof(SettlementSharedResourcesGameplayFeature).Assembly,
                typeof(BuildingWorkerAssignmentState).Assembly,
                typeof(SettlementWorkerTag).Assembly,
                typeof(BuildingConstructionCompletedEvent).Assembly);
            SW.Initialize();
        }

        public SW.Entity CreateSharedResources(int capacity = 0, int wood = 0, int stone = 0)
        {
            var entity = SW.NewEntity<Default>();
            entity.Set<SettlementResourceStorageTag>();
            entity.Set(new SettlementSharedResources { Capacity = capacity });
            ref var rows = ref entity.Add<SW.Multi<SettlementStoredResource>>();
            rows.Add(new SettlementStoredResource(ResourceCatalog.WoodId, wood));
            rows.Add(new SettlementStoredResource(ResourceCatalog.StoneId, stone));
            rows.Add(new SettlementStoredResource(ResourceCatalog.PlanksId, 0));
            rows.Add(new SettlementStoredResource(ResourceCatalog.SimplePartsId, 0));
            rows.Add(new SettlementStoredResource(ResourceCatalog.RepairKitsId, 0));
            rows.Add(new SettlementStoredResource(ResourceCatalog.FoodId, 0));
            rows.Add(new SettlementStoredResource(ResourceCatalog.FuelId, 0));
            rows.Add(new SettlementStoredResource(ResourceCatalog.ResearchDataId, 0));
            rows.Add(new SettlementStoredResource(ResourceCatalog.MedicineId, 0));
            return entity;
        }

        public SW.Entity CreateFinishedBuilding()
        {
            return SW.NewEntity<Default>();
        }

        public void Dispose()
        {
            if (SW.Status != WorldStatus.NotCreated)
                SW.Destroy();
        }
    }
}
