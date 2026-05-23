using StaticMlp.Features.CampFlow;
using NUnit.Framework;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using UnityEngine;

namespace StaticMlp.Tests.Combat
{
    public sealed class BuildingContextPanelPresentationTests
    {
        [Test]
        public void Build_CompletedWorkbench_UsesCatalogPriorityForPrimaryAction()
        {
            using var scope = new CampFlowPresentationClientWorldScope();
            CW.SetResource(new SettlementBuildingOperationOpenIntent());

            var state = BuildCompletedBuildingState(scope, BuildingCatalogData.WorkbenchId);

            Assert.That(state.PrimaryBuildingAction.Kind, Is.EqualTo(BuildingInteractionKind.OpenProductionQueue));
            Assert.That(state.PrimaryBuildingAction.Label, Is.EqualTo(FindInteractionDisplayName(BuildingCatalogData.WorkbenchId, BuildingInteractionKind.OpenProductionQueue)));
        }

        [Test]
        public void Build_CompletedExtractionBuilding_UsesCatalogPriorityForPrimaryAction()
        {
            using var scope = new CampFlowPresentationClientWorldScope();
            CW.SetResource(new SettlementBuildingOperationOpenIntent());

            var state = BuildCompletedBuildingState(scope, BuildingCatalogData.LumberCampId);

            Assert.That(state.PrimaryBuildingAction.Kind, Is.EqualTo(BuildingInteractionKind.Extract));
        }

        [Test]
        public void Build_CompletedStockpile_UsesCatalogPriorityForPrimaryAction()
        {
            using var scope = new CampFlowPresentationClientWorldScope();
            CW.SetResource(new SettlementBuildingOperationOpenIntent());

            var state = BuildCompletedBuildingState(scope, BuildingCatalogData.StockpileId);

            Assert.That(state.PrimaryBuildingAction.Kind, Is.EqualTo(BuildingInteractionKind.StoreItems));
        }

        [Test]
        public void Build_CompletedCampCore_UsesPrioritizedOpenDetailsFallback()
        {
            using var scope = new CampFlowPresentationClientWorldScope();
            CW.SetResource(new SettlementBuildingOperationOpenIntent());

            var state = BuildCompletedBuildingState(scope, BuildingCatalogData.CampCoreId);

            Assert.That(state.PrimaryBuildingAction.Kind, Is.EqualTo(BuildingInteractionKind.OpenDetails));
        }

        [Test]
        public void ResolveSummary_ExtractionInteraction_UsesCapabilityProvider()
        {
            var definition = BuildingCatalogData.Get(BuildingCatalogData.LumberCampId);

            var summary = BuildingActionPresentationCatalog.ResolveSummary(BuildingInteractionKind.Extract, in definition);

            Assert.That(summary, Does.Contain("Output: Wood"));
            Assert.That(summary, Does.Contain("Buffer capacity: 40"));
            Assert.That(summary, Does.Contain("Worker slots: 2"));
        }

        [Test]
        public void Build_OpenedInteraction_UsesCatalogInteractionDisplayName()
        {
            using var scope = new CampFlowPresentationClientWorldScope();
            CW.SetResource(new SettlementBuildingOperationOpenIntent());

            var state = BuildCompletedBuildingState(
                scope,
                BuildingCatalogData.WorkbenchId,
                BuildingInteractionKind.SetRecipe);

            Assert.That(state.OpenedBuildingActionLabel, Is.EqualTo(FindInteractionDisplayName(BuildingCatalogData.WorkbenchId, BuildingInteractionKind.SetRecipe)));
        }

        private static BuildingContextPanelState BuildCompletedBuildingState(
            CampFlowPresentationClientWorldScope scope,
            BuildingId buildingId,
            BuildingInteractionKind openedInteraction = BuildingInteractionKind.None)
        {
            var site = scope.CreateConstructionSite(
                buildingId,
                ConstructionPhase.Completed,
                woodRequired: 1,
                woodDelivered: 1,
                stoneRequired: 1,
                stoneDelivered: 1,
                progress01: 1f,
                position: Vector3.zero);
            scope.RefreshProjections();

            if (openedInteraction != BuildingInteractionKind.None)
            {
                ref var intent = ref CW.GetResource<SettlementBuildingOperationOpenIntent>();
                intent.Set(site.GID, openedInteraction);
            }

            return BuildingContextPanelPresentation.Build(new SettlementContextPanelSession
            {
                Mode = SettlementContextPanelMode.Building,
                FocusedSite = site.GID,
                WorkerAnchorId = SettlementAnchorCatalog.HomeCampId
            });
        }

        private static string FindInteractionDisplayName(BuildingId buildingId, BuildingInteractionKind kind)
        {
            var definition = BuildingCatalogData.Get(buildingId);
            for (var i = 0; i < definition.Interactions.Length; i++)
            {
                if (definition.Interactions[i].Kind == kind)
                    return definition.Interactions[i].DisplayName;
            }

            throw new System.InvalidOperationException($"Building {buildingId.Value} is missing interaction {kind}.");
        }
    }
}
