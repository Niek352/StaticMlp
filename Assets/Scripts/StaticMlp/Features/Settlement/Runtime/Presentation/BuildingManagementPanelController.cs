using System;
using Code.EcsUi.Mvc;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public sealed class BuildingManagementPanelController
        : ControllerBase<BuildingManagementPanelView>
    {
        private BuildingPanelState _sourceState;
        private BuildingPanelState _displayState;
        private EntityGID _workbenchActionTarget;
        private int _workbenchActionIndex;

        public BuildingManagementPanelController(
            ViewFactoryMethod<BuildingManagementPanelView> viewFactory,
            BuildingManagementPanelBridgeSystem bridge)
            : base(viewFactory)
        {
            AddModule(new BridgeSystemBinding<BuildingManagementPanelBridgeSystem, BuildingManagementPanelController>(this, bridge));
        }

        public override ViewLayer Layer => ViewLayer.Persistent;
        public override int? PersistentSortOrder => 110;

        public void Apply(in BuildingPanelState state)
        {
            _sourceState = state;
            ClampWorkbenchActionSelection(in _sourceState);
            _displayState = BuildDisplayState(in _sourceState);
            View.Render(in _displayState);
        }

        protected override void OnViewInstantiated()
        {
            View.Bind(HandlePrimaryAction, HandleSecondaryAction, HandleClose);
        }

        public override void Dispose()
        {
            if (View != null)
                View.Unbind();

            base.Dispose();
        }

        private void HandlePrimaryAction()
        {
            HandlePanelAction(_displayState.PrimaryAction);
        }

        private void HandleSecondaryAction()
        {
            HandlePanelAction(_displayState.SecondaryAction);
        }

        private void HandlePanelAction(in BuildingPanelAction action)
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
                    CycleWorkbenchAction();
                    return;
                default:
                    throw new InvalidOperationException($"Unsupported building panel action {action.Kind}.");
            }
        }

        private BuildingPanelState BuildDisplayState(in BuildingPanelState source)
        {
            if (source.Kind != BuildingPanelKind.WorkbenchPanel)
                return source;

            var display = source;
            var actionCount = CountWorkbenchActions(in source);
            var selected = GetWorkbenchAction(in source, _workbenchActionIndex);
            display.PrimaryAction = new BuildingPanelAction(
                selected.Kind,
                $"Do: {selected.Label}",
                selected.Enabled,
                selected.DisabledReason,
                selected.Target,
                selected.Worker,
                selected.SlotIndex,
                selected.Resource,
                selected.Amount,
                selected.RecipeId);
            display.SecondaryAction = new BuildingPanelAction(
                BuildingPanelActionKind.CycleWorkbenchAction,
                actionCount > 1 ? "Next Action" : "No More Actions",
                enabled: actionCount > 1,
                disabledReason: actionCount > 1 ? string.Empty : "Only one Workbench action is available.",
                source.Target);
            return display;
        }

        private void ClampWorkbenchActionSelection(in BuildingPanelState state)
        {
            if (state.Kind != BuildingPanelKind.WorkbenchPanel)
                return;

            if (_workbenchActionTarget != state.Target)
            {
                _workbenchActionTarget = state.Target;
                _workbenchActionIndex = 0;
            }

            var actionCount = CountWorkbenchActions(in state);
            if (actionCount == 0)
                throw new InvalidOperationException($"Workbench panel for {state.Target.Raw} has no available actions.");

            if (_workbenchActionIndex >= actionCount)
                _workbenchActionIndex = 0;
        }

        private void CycleWorkbenchAction()
        {
            var actionCount = CountWorkbenchActions(in _sourceState);
            if (actionCount <= 1)
                throw new InvalidOperationException("Workbench action cycling requires at least two actions.");

            _workbenchActionIndex = (_workbenchActionIndex + 1) % actionCount;
            _displayState = BuildDisplayState(in _sourceState);
            View.Render(in _displayState);
        }

        private static int CountWorkbenchActions(in BuildingPanelState state)
        {
            var count = 0;
            if (state.PrimaryAction.IsDefined)
                count++;

            if (state.SecondaryAction.IsDefined)
                count++;

            if (TryCreateSetProductionRecipeAction(in state.Workbench, out _))
                count++;

            return count;
        }

        private static BuildingPanelAction GetWorkbenchAction(in BuildingPanelState state, int index)
        {
            var current = 0;
            if (state.PrimaryAction.IsDefined)
            {
                if (current == index)
                    return state.PrimaryAction;

                current++;
            }

            if (state.SecondaryAction.IsDefined)
            {
                if (current == index)
                    return state.SecondaryAction;

                current++;
            }

            if (TryCreateSetProductionRecipeAction(in state.Workbench, out var recipeAction))
            {
                if (current == index)
                    return recipeAction;
            }

            throw new InvalidOperationException($"Unsupported Workbench action index {index}.");
        }

        private static bool TryCreateSetProductionRecipeAction(
            in WorkbenchPanelState state,
            out BuildingPanelAction action)
        {
            if (!TryFindNextRecipeChoice(in state, out var recipeId))
            {
                action = default;
                return false;
            }

            var recipe = ProductionRecipeCatalog.Get(new ProductionRecipeId(recipeId));
            action = new BuildingPanelAction(
                BuildingPanelActionKind.SetProductionRecipe,
                $"Set {recipe.Code}",
                enabled: true,
                disabledReason: string.Empty,
                state.Target,
                recipeId: recipeId);
            return true;
        }

        private static bool TryFindNextRecipeChoice(in WorkbenchPanelState state, out ushort recipeId)
        {
            if (state.RecipeChoices.Length <= 1)
            {
                recipeId = 0;
                return false;
            }

            var activeIndex = -1;
            for (var i = 0; i < state.RecipeChoices.Length; i++)
            {
                if (!state.RecipeChoices[i].IsActive)
                    continue;

                activeIndex = i;
                break;
            }

            if (activeIndex < 0)
                throw new InvalidOperationException(
                    $"Workbench panel state for {state.Target.Raw} does not contain the active recipe {state.ActiveRecipeId}.");

            var nextIndex = (activeIndex + 1) % state.RecipeChoices.Length;
            recipeId = state.RecipeChoices[nextIndex].RecipeId;
            return true;
        }

        private static void SendDeposit(EntityGID target)
        {
            if (!target.TryUnpack<ClientCoreWT>(out var site))
                throw new InvalidOperationException($"Deposit construction target {target.Raw} is not a client entity.");

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

        private static void HandleClose()
        {
            ref var feedback = ref CW.GetResource<SettlementTransferFeedbackState>();
            feedback.Clear();
            ref var session = ref CW.GetResource<BuildingPanelSession>();
            session.Close();
        }
    }
}
