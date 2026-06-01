using System;
using Aspid.MVVM;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    [ViewModel]
    public sealed partial class BuildingManagementPanelViewModel
    {
        [OneWayBind] private bool _isOpen;
        [OneWayBind] private string _summary;
        [OneWayBind] private string _primaryActionLabel;
        [OneWayBind] private string _secondaryActionLabel;
        [OneWayBind] private bool _primaryActionVisible;
        [OneWayBind] private bool _secondaryActionVisible;

        private BuildingPanelState _sourceState;
        private BuildingPanelState _displayState;
        private EntityGID _workbenchActionTarget;
        private int _workbenchActionIndex;

        public void Apply(in BuildingManagementPanelViewData data)
        {
            _sourceState = data.State;
            ClampWorkbenchActionSelection(in _sourceState);
            ApplyDisplayState(BuildDisplayState(in _sourceState));
        }

        [RelayCommand(CanExecute = nameof(CanPrimaryAction))]
        private void PrimaryAction()
        {
            HandlePanelAction(_displayState.PrimaryAction);
        }

        [RelayCommand(CanExecute = nameof(CanSecondaryAction))]
        private void SecondaryAction()
        {
            HandlePanelAction(_displayState.SecondaryAction);
        }

        [RelayCommand]
        private void Close()
        {
            CW.SendEvent(new BuildingManagementPanelCloseIntent());
        }

        private bool CanPrimaryAction()
        {
            return _displayState.PrimaryAction.IsDefined && _displayState.PrimaryAction.Enabled;
        }

        private bool CanSecondaryAction()
        {
            return _displayState.SecondaryAction.IsDefined && _displayState.SecondaryAction.Enabled;
        }

        private void HandlePanelAction(in BuildingPanelAction action)
        {
            if (!action.IsDefined)
                throw new InvalidOperationException("Building panel action requires a defined action.");

            if (!action.Enabled)
                throw new InvalidOperationException($"Building panel action {action.Kind} is disabled: {action.DisabledReason}");

            if (action.Kind == BuildingPanelActionKind.CycleWorkbenchAction)
            {
                CycleWorkbenchAction();
                return;
            }

            CW.SendEvent(new BuildingManagementPanelActionIntent(action));
        }

        private void ApplyDisplayState(in BuildingPanelState state)
        {
            _displayState = state;
            IsOpen = state.IsOpen;
            Summary = BuildSummary(in state);
            PrimaryActionVisible = state.PrimaryAction.IsDefined;
            SecondaryActionVisible = state.SecondaryAction.IsDefined;
            PrimaryActionLabel = state.PrimaryAction.IsDefined ? state.PrimaryAction.Label : string.Empty;
            SecondaryActionLabel = state.SecondaryAction.IsDefined ? state.SecondaryAction.Label : string.Empty;
            PrimaryActionCommand.NotifyCanExecuteChanged();
            SecondaryActionCommand.NotifyCanExecuteChanged();
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
                throw new InvalidOperationException($"Workbench panel for {state.Target} has no available actions.");

            if (_workbenchActionIndex >= actionCount)
                _workbenchActionIndex = 0;
        }

        private void CycleWorkbenchAction()
        {
            var actionCount = CountWorkbenchActions(in _sourceState);
            if (actionCount <= 1)
                throw new InvalidOperationException("Workbench action cycling requires at least two actions.");

            _workbenchActionIndex = (_workbenchActionIndex + 1) % actionCount;
            ApplyDisplayState(BuildDisplayState(in _sourceState));
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

            action = new BuildingPanelAction(
                BuildingPanelActionKind.SetProductionRecipe,
                $"Set Recipe {recipeId}",
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
                    $"Workbench panel state for {state.Target} does not contain the active recipe {state.ActiveRecipeId}.");

            var nextIndex = (activeIndex + 1) % state.RecipeChoices.Length;
            recipeId = state.RecipeChoices[nextIndex].RecipeId;
            return true;
        }

        private static string BuildSummary(in BuildingPanelState state)
        {
            var title = string.IsNullOrEmpty(state.Title) ? "Building" : state.Title;
            var summary = $"{title}\nKind: {state.Kind}";

            if (state.PrimaryAction.IsDefined)
                summary += $"\nPrimary: {FormatActionStatus(in state.PrimaryAction)}";

            if (state.SecondaryAction.IsDefined)
                summary += $"\nSecondary: {FormatActionStatus(in state.SecondaryAction)}";

            if (!string.IsNullOrEmpty(state.TransferFeedbackMessage))
                summary += $"\nFeedback: {state.TransferFeedbackMessage}";

            return summary;
        }

        private static string FormatActionStatus(in BuildingPanelAction action)
        {
            if (action.Enabled)
                return $"{action.Label} (ready)";

            return string.IsNullOrEmpty(action.DisabledReason)
                ? $"{action.Label} (locked)"
                : $"{action.Label} (locked: {action.DisabledReason})";
        }
    }
}
