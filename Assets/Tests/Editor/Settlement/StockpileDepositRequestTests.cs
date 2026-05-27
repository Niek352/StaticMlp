using NUnit.Framework;
using StaticMlp.Features.ResourcesInventoryMinimal;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;
using UnityEngine;

namespace StaticMlp.Tests.Settlement
{
    public sealed class StockpileDepositRequestTests
    {
        [Test]
        public void DepositCarriedResourcesToStockpileHandler_TransfersCarriedResourcesWithinCapacity()
        {
            using var scope = new SettlementOperationTestWorldScope();
            var peer = new NetworkPeerId(1);
            var storage = scope.CreateSharedResources(capacity: 10, wood: 3);
            var stockpile = scope.CreateFinishedStockpile(Vector3.zero);
            var player = scope.CreatePlayer(peer, Vector3.zero);
            ResourcesInventoryAccess.Add(player, new ResourceAmount(ResourceCatalog.WoodId, 6));
            ResourcesInventoryAccess.Add(player, new ResourceAmount(ResourceCatalog.StoneId, 4));

            var result = new DepositCarriedResourcesToStockpileHandler().Handle(
                peer,
                new DepositCarriedResourcesToStockpileRequestEvent(stockpile.GID));

            Assert.That(result.Status, Is.EqualTo(RequestStatus.Accepted));
            Assert.That(result.TransferredAmount, Is.EqualTo(7));
            Assert.That(SettlementSharedResourcesAccess.GetAmount(storage, ResourceCatalog.WoodId), Is.EqualTo(9));
            Assert.That(SettlementSharedResourcesAccess.GetAmount(storage, ResourceCatalog.StoneId), Is.EqualTo(1));
            Assert.That(ResourcesInventoryAccess.GetAmount(player, ResourceCatalog.WoodId), Is.EqualTo(0));
            Assert.That(ResourcesInventoryAccess.GetAmount(player, ResourceCatalog.StoneId), Is.EqualTo(3));
            Assert.That(SettlementSharedResourcesAccess.TotalUsed(storage), Is.EqualTo(10));
        }

        [Test]
        public void DepositCarriedResourcesToStockpileHandler_RejectsNonStockpileTarget()
        {
            using var scope = new SettlementOperationTestWorldScope();
            var peer = new NetworkPeerId(1);
            scope.CreateSharedResources(capacity: 10);
            var target = scope.CreateFinishedBuilding();
            var player = scope.CreatePlayer(peer, Vector3.zero);
            ResourcesInventoryAccess.Add(player, new ResourceAmount(ResourceCatalog.WoodId, 1));

            var result = new DepositCarriedResourcesToStockpileHandler().Handle(
                peer,
                new DepositCarriedResourcesToStockpileRequestEvent(target.GID));

            Assert.That(result.Status, Is.EqualTo(RequestStatus.Rejected));
            Assert.That(result.TransferredAmount, Is.EqualTo(0));
            Assert.That(ResourcesInventoryAccess.GetAmount(player, ResourceCatalog.WoodId), Is.EqualTo(1));
        }
    }
}
