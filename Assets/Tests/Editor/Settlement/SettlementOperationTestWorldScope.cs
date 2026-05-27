using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.ResourcesInventoryMinimal;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

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
                typeof(ResourcesInventoryMinimalGameplayFeature).Assembly,
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

        public SW.Entity CreateFinishedStockpile(Vector3 position)
        {
            var entity = CreateFinishedBuilding();
            entity.Set<FinishedBuildingTag>();
            entity.Set(new ConstructionSiteState
            {
                BuildingId = BuildingCatalogData.StockpileId.Value,
                Phase = ConstructionPhase.Completed
            });
            entity.Set(new ConstructionTransform
            {
                Position = position,
                Rotation = Quaternion.identity
            });
            entity.Set(new StockpileOperationState
            {
                ContributedCapacity = 200,
                Enabled = true
            });
            return entity;
        }

        public SW.Entity CreatePlayer(NetworkPeerId owner, Vector3 position)
        {
            var player = SW.NewEntity<Default>();
            player.Set<PlayerTag>();
            player.Set(new NetworkIdentity
            {
                Owner = owner,
                Authority = NetworkAuthority.Owner,
                NetworkArchetypeId = 0
            });
            player.Set(new CharacterNetState
            {
                Position = position,
                Rotation = Quaternion.identity
            });
            ResourcesInventoryAccess.Initialize(player, ResourcesInventory.MAX_SLOTS);
            return player;
        }

        public void Dispose()
        {
            if (SW.Status != WorldStatus.NotCreated)
                SW.Destroy();
        }
    }
}
