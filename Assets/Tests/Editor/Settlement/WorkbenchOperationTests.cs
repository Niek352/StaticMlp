using System;
using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Game;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;
using UnityEngine;

namespace StaticMlp.Tests.Settlement
{
    public sealed class WorkbenchOperationTests
    {
        [Test]
        public void ProductionRecipeCatalog_ContainsUniqueValidWorkbenchRecipes()
        {
            Assert.DoesNotThrow(() => ProductionRecipeCatalog.Validate(ProductionRecipeCatalog.All));
            Assert.That(ProductionRecipeCatalog.All.Count, Is.EqualTo(3));
            Assert.That(ProductionRecipeCatalog.Get(ProductionRecipeCatalog.WorkbenchPlanksId).StationId, Is.EqualTo(ProductionStationIds.Workbench));
            Assert.That(ProductionRecipeCatalog.Get(ProductionRecipeCatalog.WorkbenchPlanksId).Outputs[0].Id, Is.EqualTo(ResourceCatalog.PlanksId));
            Assert.That(ProductionRecipeCatalog.Get(ProductionRecipeCatalog.WorkbenchSimplePartsId).Outputs[0].Id, Is.EqualTo(ResourceCatalog.SimplePartsId));
            Assert.That(ProductionRecipeCatalog.Get(ProductionRecipeCatalog.WorkbenchRepairKitsId).Outputs[0].Id, Is.EqualTo(ResourceCatalog.RepairKitsId));
        }

        [Test]
        public void ProductionRecipeCatalog_RejectsDuplicateRecipeIds()
        {
            var recipes = new[]
            {
                new ProductionRecipeDefinition(
                    ProductionStationIds.Workbench,
                    ProductionRecipeCatalog.WorkbenchPlanksId,
                    "one",
                    new[] { new ResourceAmount(ResourceCatalog.WoodId, 1) },
                    new[] { new ResourceAmount(ResourceCatalog.PlanksId, 1) },
                    1f),
                new ProductionRecipeDefinition(
                    ProductionStationIds.Workbench,
                    ProductionRecipeCatalog.WorkbenchPlanksId,
                    "two",
                    new[] { new ResourceAmount(ResourceCatalog.WoodId, 1) },
                    new[] { new ResourceAmount(ResourceCatalog.PlanksId, 1) },
                    1f)
            };

            Assert.Throws<InvalidOperationException>(() => ProductionRecipeCatalog.Validate(recipes));
        }

        [Test]
        public void ServerWorkbenchBootstrapSystem_AddsDefaultOperationState()
        {
            using var scope = new SettlementOperationTestWorldScope();
            var finished = scope.CreateFinishedBuilding();
            var system = new ServerWorkbenchBootstrapSystem();
            system.Init();

            SW.SendEvent(new BuildingConstructionCompletedEvent(
                finished.GID,
                BuildingCatalogData.WorkbenchId,
                SettlementAnchorCatalog.HomeCampId,
                Vector3.zero,
                Quaternion.identity));
            system.Update();
            system.Destroy();

            var state = finished.Read<ProductionStationOperationState>();
            Assert.That(state.Enabled, Is.True);
            Assert.That(state.Station, Is.EqualTo(ProductionStationIds.Workbench));
            Assert.That(state.ActiveRecipe, Is.EqualTo(ProductionRecipeCatalog.WorkbenchPlanksId));
            Assert.That(state.WorkerSlotCount, Is.EqualTo(2));
            Assert.That(state.WorkDone, Is.EqualTo(0f));
            Assert.That(ProductionStationResourceAccess.GetInput(finished, ResourceCatalog.WoodId), Is.EqualTo(0));
            Assert.That(ProductionStationResourceAccess.GetOutput(finished, ResourceCatalog.PlanksId), Is.EqualTo(0));
            Assert.That(finished.Ref<SW.Multi<ProductionStationInputResource>>().Length, Is.EqualTo(5));
            Assert.That(finished.Ref<SW.Multi<ProductionStationOutputResource>>().Length, Is.EqualTo(3));
        }

        [Test]
        public void ProductionStationResourceAccess_ReadsArbitraryRecipeResourceRows()
        {
            using var scope = new SettlementOperationTestWorldScope();
            var station = scope.CreateFinishedBuilding();
            station.Set(new ProductionStationOperationState
            {
                StationId = ProductionStationIds.Workbench.Value,
                ActiveRecipeId = ProductionRecipeCatalog.WorkbenchRepairKitsId.Value,
                Enabled = true,
                WorkerSlotCount = 1
            });
            ProductionStationResourceAccess.InitializeRows(station, ProductionStationIds.Workbench);
            ref var inputs = ref station.Ref<SW.Multi<ProductionStationInputResource>>();
            SetInput(ref inputs, ResourceCatalog.SimplePartsId, 2);
            SetInput(ref inputs, ResourceCatalog.FuelId, 1);
            ref var outputs = ref station.Ref<SW.Multi<ProductionStationOutputResource>>();
            SetOutput(ref outputs, ResourceCatalog.RepairKitsId, 1);

            Assert.That(ProductionStationResourceAccess.GetInput(station, ResourceCatalog.SimplePartsId), Is.EqualTo(2));
            Assert.That(ProductionStationResourceAccess.GetOutput(station, ResourceCatalog.RepairKitsId), Is.EqualTo(1));
        }

        [Test]
        public void ServerProductionStationProcessingSystem_ConsumesSharedInputsAndCreatesOutput()
        {
            using var scope = new SettlementOperationTestWorldScope();
            var storage = scope.CreateSharedResources(capacity: 100, wood: 2, fuel: 1);
            var station = CreateProductionWorkbench(Vector3.zero);
            CreateAssignedWorker(station.GID);
            SW.GetResource<SimulationTime>().FixedStepSeconds = 20f;

            new ServerProductionStationProcessingSystem().Update();

            Assert.That(SettlementSharedResourcesAccess.GetAmount(storage, ResourceCatalog.WoodId), Is.EqualTo(0));
            Assert.That(SettlementSharedResourcesAccess.GetAmount(storage, ResourceCatalog.FuelId), Is.EqualTo(0));
            Assert.That(ProductionStationResourceAccess.GetInput(station, ResourceCatalog.WoodId), Is.EqualTo(0));
            Assert.That(ProductionStationResourceAccess.GetOutput(station, ResourceCatalog.PlanksId), Is.EqualTo(1));
            Assert.That(station.Read<ProductionStationOperationState>().WorkDone, Is.EqualTo(0f));
        }

        [Test]
        public void ServerProductionStationProcessingSystem_DoesNotReserveInputsWhenOutputBufferIsFull()
        {
            using var scope = new SettlementOperationTestWorldScope();
            var storage = scope.CreateSharedResources(capacity: 100, wood: 2, fuel: 1);
            var station = CreateProductionWorkbench(Vector3.zero);
            CreateAssignedWorker(station.GID);
            ref var outputs = ref station.Ref<SW.Multi<ProductionStationOutputResource>>();
            SetOutput(ref outputs, ResourceCatalog.PlanksId, 24);
            SW.GetResource<SimulationTime>().FixedStepSeconds = 20f;

            new ServerProductionStationProcessingSystem().Update();

            Assert.That(SettlementSharedResourcesAccess.GetAmount(storage, ResourceCatalog.WoodId), Is.EqualTo(2));
            Assert.That(SettlementSharedResourcesAccess.GetAmount(storage, ResourceCatalog.FuelId), Is.EqualTo(1));
            Assert.That(ProductionStationResourceAccess.GetInput(station, ResourceCatalog.WoodId), Is.EqualTo(0));
            Assert.That(ProductionStationResourceAccess.GetOutput(station, ResourceCatalog.PlanksId), Is.EqualTo(24));
            Assert.That(station.Read<ProductionStationOperationState>().WorkDone, Is.EqualTo(0f));
            Assert.That(station.Read<ProductionStationOperationState>().BlockedReason, Is.EqualTo(ProductionStationBlockedReason.FullOutputBuffer));
        }

        [Test]
        public void ServerProductionStationProcessingSystem_SetsBlockedStateWhenFuelIsMissing()
        {
            using var scope = new SettlementOperationTestWorldScope();
            var storage = scope.CreateSharedResources(capacity: 100, wood: 2, fuel: 0);
            var station = CreateProductionWorkbench(Vector3.zero);
            CreateAssignedWorker(station.GID);
            SW.GetResource<SimulationTime>().FixedStepSeconds = 20f;

            new ServerProductionStationProcessingSystem().Update();

            Assert.That(SettlementSharedResourcesAccess.GetAmount(storage, ResourceCatalog.WoodId), Is.EqualTo(2));
            Assert.That(SettlementSharedResourcesAccess.GetAmount(storage, ResourceCatalog.FuelId), Is.EqualTo(0));
            Assert.That(ProductionStationResourceAccess.GetOutput(station, ResourceCatalog.PlanksId), Is.EqualTo(0));
            Assert.That(station.Read<ProductionStationOperationState>().WorkDone, Is.EqualTo(0f));
            Assert.That(station.Read<ProductionStationOperationState>().BlockedReason, Is.EqualTo(ProductionStationBlockedReason.MissingFuel));
        }

        [Test]
        public void ClaimProductionOutputHandler_TransfersStationOutputWithinSharedStorageCapacity()
        {
            using var scope = new SettlementOperationTestWorldScope();
            var peer = new NetworkPeerId(1);
            var storage = scope.CreateSharedResources(capacity: 5, wood: 3);
            var station = CreateProductionWorkbench(Vector3.zero);
            scope.CreatePlayer(peer, Vector3.zero);
            ref var outputs = ref station.Ref<SW.Multi<ProductionStationOutputResource>>();
            SetOutput(ref outputs, ResourceCatalog.PlanksId, 4);
            var system = new ServerClaimProductionOutputToStorageSystem();
            system.Init();

            var result = new ClaimProductionOutputHandler().Handle(
                peer,
                new ClaimProductionOutputRequestEvent(station.GID, ResourceCatalog.PlanksId, 4));
            try { system.Update(); }
            finally { system.Destroy(); }

            Assert.That(result.Status, Is.EqualTo(RequestStatus.Accepted));
            Assert.That(result.TransferredAmount, Is.EqualTo(2));
            Assert.That(SettlementSharedResourcesAccess.GetAmount(storage, ResourceCatalog.PlanksId), Is.EqualTo(2));
            Assert.That(ProductionStationResourceAccess.GetOutput(station, ResourceCatalog.PlanksId), Is.EqualTo(2));
            Assert.That(SettlementSharedResourcesAccess.TotalUsed(storage), Is.EqualTo(5));
        }

        private static SW.Entity CreateProductionWorkbench(Vector3 position)
        {
            var station = SW.NewEntity<Default>();
            station.Set<FinishedBuildingTag>();
            station.Set(new ConstructionSiteState
            {
                BuildingId = BuildingCatalogData.WorkbenchId.Value,
                Phase = ConstructionPhase.Completed
            });
            station.Set(new ConstructionTransform
            {
                Position = position,
                Rotation = Quaternion.identity
            });
            station.Set(new ProductionStationOperationState
            {
                StationId = ProductionStationIds.Workbench.Value,
                ActiveRecipeId = ProductionRecipeCatalog.WorkbenchPlanksId.Value,
                Enabled = true,
                WorkerSlotCount = 1,
                WorkDone = 0f
            });
            ProductionStationResourceAccess.InitializeRows(station, ProductionStationIds.Workbench);
            return station;
        }

        private static SW.Entity CreateAssignedWorker(EntityGID building)
        {
            var worker = SW.NewEntity<Default>();
            worker.Set(new BuildingWorkerAssignmentState
            {
                Status = SettlementWorkerAssignmentStatus.Assigned,
                AnchorId = SettlementAnchorCatalog.HomeCampId.Value,
                Building = building,
                SlotIndex = 0
            });
            return worker;
        }

        private static void SetInput(ref SW.Multi<ProductionStationInputResource> rows, ResourceId resourceId, int amount)
        {
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Id != resourceId)
                    continue;

                ref var row = ref rows[i];
                row.Amount = amount;
                return;
            }

            throw new InvalidOperationException($"Missing workbench input resource id {resourceId.Value}.");
        }

        private static void SetOutput(ref SW.Multi<ProductionStationOutputResource> rows, ResourceId resourceId, int amount)
        {
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Id != resourceId)
                    continue;

                ref var row = ref rows[i];
                row.Amount = amount;
                return;
            }

            throw new InvalidOperationException($"Missing workbench output resource id {resourceId.Value}.");
        }
    }
}
