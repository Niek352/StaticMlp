using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public sealed class ClientBuildingManagementPanelActionIntentSystem : ISystem
    {
        private EventReceiver<ClientCoreWT, BuildingManagementPanelActionIntent> _actionIntents;

        public void Init()
        {
            _actionIntents = CW.RegisterEventReceiver<BuildingManagementPanelActionIntent>();
        }

        public void Destroy()
        {
            CW.DeleteEventReceiver(ref _actionIntents);
        }

        public void Update()
        {
            foreach (var evt in _actionIntents)
                HandlePanelAction(evt.Value.Action);
        }

        private static void HandlePanelAction(in BuildingPanelAction action)
        {
            if (!action.IsDefined)
                throw new InvalidOperationException("Building panel action requires a defined action.");

            if (!action.Enabled)
                throw new InvalidOperationException($"Building panel action {action.Kind} is disabled: {action.DisabledReason}");

            switch (action.Kind)
            {
                case BuildingPanelActionKind.DepositConstructionResources:
                    SendDeposit(action.Target);
                    return;
                case BuildingPanelActionKind.ContributeBuildWork:
                    SendBuild(action.Target);
                    return;
                case BuildingPanelActionKind.AssignWorker:
                    SendWorkerAssignment(action, assigned: true);
                    return;
                case BuildingPanelActionKind.UnassignWorker:
                    SendWorkerAssignment(action, assigned: false);
                    return;
                case BuildingPanelActionKind.CollectExtractionOutput:
                    SendCollectExtractionOutput(action);
                    return;
                case BuildingPanelActionKind.DepositCarriedResourcesToStockpile:
                    SendDepositCarriedResourcesToStockpile(action.Target);
                    return;
                case BuildingPanelActionKind.ClaimProductionOutput:
                    SendClaimProductionOutput(action);
                    return;
                case BuildingPanelActionKind.SetProductionRecipe:
                    SendSetProductionRecipe(action);
                    return;
                case BuildingPanelActionKind.CycleWorkbenchAction:
                    throw new InvalidOperationException("Workbench action cycling is view-model local and must not reach the action intent system.");
                default:
                    throw new InvalidOperationException($"Unsupported building panel action {action.Kind}.");
            }
        }

        private static void SendDeposit(EntityGID target)
        {
            if (!target.TryUnpack<ClientCoreWT>(out var site))
                throw new InvalidOperationException($"Deposit construction target {target} is not a client entity.");

            var depositRequest = new DepositConstructionResourcesRequestEvent(
                target,
                ConstructionResourcesAccess.GetProjectedRemainingResources(site));
            RequestApi.Send<DepositConstructionResourcesRequestEvent, DepositConstructionResourcesResultEvent>(depositRequest);
        }

        private static void SendBuild(EntityGID target)
        {
            var buildRequest = new BuildConstructionRequestEvent(
                target,
                ConstructionActionProfiles.PlayerBuildClickWork);
            RequestApi.Send<BuildConstructionRequestEvent, BuildConstructionResultEvent>(buildRequest);
        }

        private static void SendWorkerAssignment(in BuildingPanelAction action, bool assigned)
        {
            var request = new SetBuildingWorkerAssignmentRequestEvent(
                action.Worker,
                action.Target,
                action.SlotIndex,
                assigned);
            RequestApi.Send<SetBuildingWorkerAssignmentRequestEvent, SetBuildingWorkerAssignmentResultEvent>(request);
        }

        private static void SendCollectExtractionOutput(in BuildingPanelAction action)
        {
            var request = new CollectExtractionOutputRequestEvent(
                action.Target,
                action.Resource,
                action.Amount);
            RequestApi.Send<CollectExtractionOutputRequestEvent, CollectExtractionOutputResultEvent>(request);
        }

        private static void SendDepositCarriedResourcesToStockpile(EntityGID target)
        {
            var request = new DepositCarriedResourcesToStockpileRequestEvent(target);
            RequestApi.Send<DepositCarriedResourcesToStockpileRequestEvent, DepositCarriedResourcesToStockpileResultEvent>(request);
        }

        private static void SendClaimProductionOutput(in BuildingPanelAction action)
        {
            var request = new ClaimProductionOutputRequestEvent(
                action.Target,
                action.Resource,
                action.Amount);
            RequestApi.Send<ClaimProductionOutputRequestEvent, ClaimProductionOutputResultEvent>(request);
        }

        private static void SendSetProductionRecipe(in BuildingPanelAction action)
        {
            var request = new SetProductionRecipeRequestEvent(
                action.Target,
                action.RecipeId);
            RequestApi.Send<SetProductionRecipeRequestEvent, SetProductionRecipeResultEvent>(request);
        }
    }
}
