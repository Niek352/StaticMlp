using System;
using NUnit.Framework;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Tests.Settlement
{
    public sealed class WorkbenchOperationTests
    {
        [Test]
        public void WorkbenchRecipeCatalog_ContainsUniqueValidRecipes()
        {
            Assert.DoesNotThrow(() => WorkbenchRecipeCatalog.Validate(WorkbenchRecipeCatalog.All));
            Assert.That(WorkbenchRecipeCatalog.All.Count, Is.EqualTo(3));
            Assert.That(WorkbenchRecipeCatalog.Get(WorkbenchRecipeCatalog.PlanksId).Outputs[0].Id, Is.EqualTo(ResourceCatalog.PlanksId));
            Assert.That(WorkbenchRecipeCatalog.Get(WorkbenchRecipeCatalog.SimplePartsId).Outputs[0].Id, Is.EqualTo(ResourceCatalog.SimplePartsId));
            Assert.That(WorkbenchRecipeCatalog.Get(WorkbenchRecipeCatalog.RepairKitsId).Outputs[0].Id, Is.EqualTo(ResourceCatalog.RepairKitsId));
        }

        [Test]
        public void WorkbenchRecipeCatalog_RejectsDuplicateRecipeIds()
        {
            var recipes = new[]
            {
                new WorkbenchRecipeDefinition(
                    WorkbenchRecipeCatalog.PlanksId,
                    "one",
                    new[] { new ResourceAmount(ResourceCatalog.WoodId, 1) },
                    new[] { new ResourceAmount(ResourceCatalog.PlanksId, 1) },
                    1f),
                new WorkbenchRecipeDefinition(
                    WorkbenchRecipeCatalog.PlanksId,
                    "two",
                    new[] { new ResourceAmount(ResourceCatalog.WoodId, 1) },
                    new[] { new ResourceAmount(ResourceCatalog.PlanksId, 1) },
                    1f)
            };

            Assert.Throws<InvalidOperationException>(() => WorkbenchRecipeCatalog.Validate(recipes));
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

            var state = finished.Read<WorkbenchOperationState>();
            Assert.That(state.Enabled, Is.True);
            Assert.That(state.ActiveRecipe, Is.EqualTo(WorkbenchRecipeCatalog.PlanksId));
            Assert.That(state.WorkerSlotCount, Is.EqualTo(2));
            Assert.That(state.WorkDone, Is.EqualTo(0f));
            Assert.That(WorkbenchResourceAccess.GetInput(finished, ResourceCatalog.WoodId), Is.EqualTo(0));
            Assert.That(WorkbenchResourceAccess.GetOutput(finished, ResourceCatalog.PlanksId), Is.EqualTo(0));
            Assert.That(finished.Ref<SW.Multi<WorkbenchInputResource>>().Length, Is.EqualTo(4));
            Assert.That(finished.Ref<SW.Multi<WorkbenchOutputResource>>().Length, Is.EqualTo(3));
        }

        [Test]
        public void WorkbenchResourceAccess_ReadsArbitraryRecipeResourceRows()
        {
            using var scope = new SettlementOperationTestWorldScope();
            var workbench = scope.CreateFinishedBuilding();
            workbench.Set(new WorkbenchOperationState
            {
                ActiveRecipeId = WorkbenchRecipeCatalog.RepairKitsId.Value,
                Enabled = true,
                WorkerSlotCount = 1
            });
            WorkbenchResourceAccess.InitializeRows(workbench);
            ref var inputs = ref workbench.Ref<SW.Multi<WorkbenchInputResource>>();
            SetInput(ref inputs, ResourceCatalog.SimplePartsId, 2);
            ref var outputs = ref workbench.Ref<SW.Multi<WorkbenchOutputResource>>();
            SetOutput(ref outputs, ResourceCatalog.RepairKitsId, 1);

            Assert.That(WorkbenchResourceAccess.GetInput(workbench, ResourceCatalog.SimplePartsId), Is.EqualTo(2));
            Assert.That(WorkbenchResourceAccess.GetOutput(workbench, ResourceCatalog.RepairKitsId), Is.EqualTo(1));
        }

        private static void SetInput(ref SW.Multi<WorkbenchInputResource> rows, ResourceId resourceId, int amount)
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

        private static void SetOutput(ref SW.Multi<WorkbenchOutputResource> rows, ResourceId resourceId, int amount)
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
