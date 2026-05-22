using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Tests.Settlement
{
    public sealed class ExtractionOperationTests
    {
        [Test]
        public void ExtractionRules_FillsBufferAndPausesWhenFull()
        {
            var state = new ExtractionOperationState
            {
                OutputResourceId = ResourceCatalog.WoodId.Value,
                OutputBufferAmount = 38,
                OutputBufferCapacity = 40,
                Enabled = true,
                WorkerSlotCount = 2
            };

            Assert.That(ExtractionRules.FillBuffer(ref state, assignedWorkerCount: 2), Is.EqualTo(2));
            Assert.That(state.OutputBufferAmount, Is.EqualTo(40));
            Assert.That(ExtractionRules.FillBuffer(ref state, assignedWorkerCount: 2), Is.EqualTo(0));
            Assert.That(state.OutputBufferAmount, Is.EqualTo(40));
        }

        [Test]
        public void ExtractionRules_MapsLumberCampAndStoneMineToCorrectResourceIds()
        {
            Assert.That(ExtractionRules.TryGetOutputResource(BuildingCatalogData.LumberCampId, out var lumberOutput), Is.True);
            Assert.That(lumberOutput, Is.EqualTo(ResourceCatalog.WoodId));

            Assert.That(ExtractionRules.TryGetOutputResource(BuildingCatalogData.StoneMineId, out var mineOutput), Is.True);
            Assert.That(mineOutput, Is.EqualTo(ResourceCatalog.StoneId));

            Assert.That(ExtractionRules.TryGetOutputResource(BuildingCatalogData.StockpileId, out _), Is.False);
        }

        [Test]
        public void ServerExtractionBootstrapSystem_AddsLumberCampOperationState()
        {
            using var scope = new SettlementOperationTestWorldScope();
            var finished = scope.CreateFinishedBuilding();
            var system = new ServerExtractionBootstrapSystem();
            system.Init();

            SW.SendEvent(new BuildingConstructionCompletedEvent(
                finished.GID,
                BuildingCatalogData.LumberCampId,
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Quaternion.identity));
            try { system.Update(); }
            finally { system.Destroy(); }

            var state = finished.Read<ExtractionOperationState>();
            Assert.That(state.Enabled, Is.True);
            Assert.That(state.OutputResource, Is.EqualTo(ResourceCatalog.WoodId));
            Assert.That(state.OutputBufferAmount, Is.EqualTo(0));
            Assert.That(state.OutputBufferCapacity, Is.EqualTo(40));
            Assert.That(state.WorkerSlotCount, Is.EqualTo(2));
        }

        [Test]
        public void ServerExtractionOperationSystem_FillsFinishedBuildingBuffer()
        {
            using var scope = new SettlementOperationTestWorldScope();
            var building = scope.CreateFinishedBuilding();
            building.Set<FinishedBuildingTag>();
            building.Set(new ExtractionOperationState
            {
                OutputResourceId = ResourceCatalog.StoneId.Value,
                OutputBufferAmount = 39,
                OutputBufferCapacity = 40,
                Enabled = true,
                WorkerSlotCount = 2
            });
            var worker = SW.NewEntity<Default>();
            worker.Set<SettlementWorkerTag>();
            worker.Set(new BuildingWorkerAssignmentState
            {
                Status = SettlementWorkerAssignmentStatus.Assigned,
                AnchorId = SettlementAnchorCatalog.HomeCampId.Value,
                Building = building.GID,
                SlotIndex = 0
            });

            new ServerExtractionOperationSystem().Update();

            Assert.That(building.Read<ExtractionOperationState>().OutputBufferAmount, Is.EqualTo(40));
        }

        [Test]
        public void ServerTransferExtractionOutputToStockpileSystem_TransfersWithoutExceedingCapacity()
        {
            using var scope = new SettlementOperationTestWorldScope();
            var storage = scope.CreateSharedResources(capacity: 5, wood: 3);
            var building = scope.CreateFinishedBuilding();
            building.Set(new ExtractionOperationState
            {
                OutputResourceId = ResourceCatalog.WoodId.Value,
                OutputBufferAmount = 4,
                OutputBufferCapacity = 40,
                Enabled = true,
                WorkerSlotCount = 2
            });
            var system = new ServerTransferExtractionOutputToStockpileSystem();
            system.Init();

            SW.SendEvent(new TransferExtractionOutputToStockpileEvent(
                building.GID,
                ResourceCatalog.WoodId,
                4));
            try { system.Update(); }
            finally { system.Destroy(); }

            Assert.That(SettlementSharedResourcesAccess.GetAmount(storage, ResourceCatalog.WoodId), Is.EqualTo(5));
            Assert.That(building.Read<ExtractionOperationState>().OutputBufferAmount, Is.EqualTo(2));
            Assert.That(SettlementSharedResourcesAccess.TotalUsed(storage), Is.EqualTo(5));
        }
    }
}
