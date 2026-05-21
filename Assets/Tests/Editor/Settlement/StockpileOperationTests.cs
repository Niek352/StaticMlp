using NUnit.Framework;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Tests.Settlement
{
    public sealed class StockpileOperationTests
    {
        [Test]
        public void StockpileRules_ClampToRemainingCapacity()
        {
            Assert.That(StockpileRules.ClampToCapacity(10, 4, 20), Is.EqualTo(6));
            Assert.That(StockpileRules.ClampToCapacity(10, 10, 1), Is.EqualTo(0));
            Assert.That(StockpileRules.ClampToCapacity(10, 4, -1), Is.EqualTo(0));
        }

        [Test]
        public void SharedResources_AddRejectsOverCapacity()
        {
            using var scope = new SettlementOperationTestWorldScope();
            var storage = scope.CreateSharedResources(capacity: 5);

            Assert.That(SettlementSharedResourcesAccess.Add(storage, ResourceCatalog.WoodId, 8), Is.EqualTo(5));
            Assert.That(SettlementSharedResourcesAccess.Add(storage, ResourceCatalog.StoneId, 1), Is.EqualTo(0));
            Assert.That(SettlementSharedResourcesAccess.TotalUsed(storage), Is.EqualTo(5));
        }

        [Test]
        public void ServerStockpileOperationBootstrapSystem_AddsOperationStateAndCapacity()
        {
            using var scope = new SettlementOperationTestWorldScope();
            var storage = scope.CreateSharedResources(capacity: 0);
            var finished = scope.CreateFinishedBuilding();
            var system = new ServerStockpileOperationBootstrapSystem();
            system.Init();

            SW.SendEvent(new BuildingConstructionCompletedEvent(
                finished.GID,
                BuildingCatalogData.StockpileId,
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Quaternion.identity));
            system.Update();
            system.Destroy();

            Assert.That(finished.Has<StockpileOperationState>(), Is.True);
            Assert.That(finished.Read<StockpileOperationState>().Enabled, Is.True);
            Assert.That(finished.Read<StockpileOperationState>().ContributedCapacity, Is.EqualTo(200));
            Assert.That(storage.Read<SettlementSharedResources>().Capacity, Is.EqualTo(200));
        }
    }
}
