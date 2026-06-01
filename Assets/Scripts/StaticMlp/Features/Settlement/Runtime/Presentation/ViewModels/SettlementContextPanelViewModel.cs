using System.Text;
using Aspid.MVVM;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    [ViewModel]
    public sealed partial class SettlementContextPanelViewModel
    {
        [OneWayBind] private string _summary;
        [OneWayBind] private string _primaryActionLabel;
        [OneWayBind] private string _secondaryActionLabel;
        [OneWayBind] private bool _secondaryActionVisible;

        private SettlementContextPanelViewData _data;

        public void Apply(in SettlementContextPanelViewData data)
        {
            _data = data;
            if (data.Mode == SettlementContextPanelMode.Building)
                ApplyBuilding(in data.Building);
            else
                ApplyWorker(in data.Worker);

            PrimaryActionCommand.NotifyCanExecuteChanged();
            SecondaryActionCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanPrimaryAction))]
        private void PrimaryAction()
        {
            CW.SendEvent(new SettlementContextPanelActionIntent(isPrimary: true));
        }

        [RelayCommand(CanExecute = nameof(CanSecondaryAction))]
        private void SecondaryAction()
        {
            CW.SendEvent(new SettlementContextPanelActionIntent(isPrimary: false));
        }

        private bool CanPrimaryAction()
        {
            return _data.Mode == SettlementContextPanelMode.Building
                ? _data.Building.PrimaryBuildingAction.IsDefined && _data.Building.PrimaryBuildingAction.Enabled
                : _data.Worker.HasWorker && _data.Worker.CanToggleWorkerAssignment;
        }

        private bool CanSecondaryAction()
        {
            return _data.Mode == SettlementContextPanelMode.Building
                   && _data.Building.SecondaryBuildingAction.IsDefined
                   && _data.Building.SecondaryBuildingAction.Enabled;
        }

        private void ApplyBuilding(in BuildingContextPanelState state)
        {
            PrimaryActionLabel = state.PrimaryBuildingAction.Label;
            SecondaryActionLabel = state.SecondaryBuildingAction.Label;
            SecondaryActionVisible = state.SecondaryBuildingAction.IsDefined;
            Summary = BuildBuildingSummary(in state);
        }

        private void ApplyWorker(in WorkerContextPanelState state)
        {
            PrimaryActionLabel = state.HasWorker
                ? state.WorkerAssigned ? "Unassign" : "Assign"
                : "No worker";
            SecondaryActionLabel = string.Empty;
            SecondaryActionVisible = false;
            Summary = BuildWorkerSummary(in state);
        }

        private static string BuildBuildingSummary(in BuildingContextPanelState state)
        {
            var builder = new StringBuilder();
            builder.Append(state.BuildingDisplayName);
            builder.Append("\nPhase: ");
            builder.Append(state.ConstructionPhase);
            builder.Append("\nProgress: ");
            builder.Append((int)(state.Progress01 * 100f));
            builder.Append('%');
            AppendActionStatus(builder, "Primary", in state.PrimaryBuildingAction);
            AppendActionStatus(builder, "Secondary", in state.SecondaryBuildingAction);

            if (state.HasOpenedBuildingAction)
            {
                builder.Append("\n\n");
                builder.Append(state.OpenedBuildingActionLabel);
                builder.Append("\n");
                builder.Append(state.OpenedBuildingActionSummary);
            }

            return builder.ToString();
        }

        private static string BuildWorkerSummary(in WorkerContextPanelState state)
        {
            if (!state.HasWorker)
                return "No worker available.";

            var builder = new StringBuilder();
            var assignedCount = 0;
            for (var i = 0; i < state.Workers.Length; i++)
            {
                if (state.Workers[i].IsAssigned)
                    assignedCount++;
            }

            builder.Append("Workers: ");
            builder.Append(assignedCount);
            builder.Append(" / ");
            builder.Append(state.Workers.Length);
            builder.Append(" assigned");

            builder.Append("\nCurrent worker: ");
            builder.Append(state.WorkerAssigned ? "Assigned" : "Unassigned");
            builder.Append(state.CanToggleWorkerAssignment ? " (ready)" : " (locked)");

            if (state.WorkerBlockingReason != SettlementWorkerBlockingReason.None)
            {
                builder.Append("\nBlocked: ");
                builder.Append(state.WorkerBlockingReason);
            }

            return builder.ToString();
        }

        private static void AppendActionStatus(
            StringBuilder builder,
            string slot,
            in BuildingAvailableActionPresentation action)
        {
            if (!action.IsDefined)
                return;

            builder.Append('\n');
            builder.Append(slot);
            builder.Append(": ");
            builder.Append(action.Label);
            builder.Append(action.Enabled ? " (ready)" : " (locked)");

            if (!string.IsNullOrEmpty(action.InputHint))
            {
                builder.Append(" via ");
                builder.Append(action.InputHint);
            }

            if (!string.IsNullOrEmpty(action.EffectDescription))
            {
                builder.Append("\nEffect: ");
                builder.Append(action.EffectDescription);
            }

            if (!action.Enabled && !string.IsNullOrEmpty(action.DisabledReason))
            {
                builder.Append("\nBlocked: ");
                builder.Append(action.DisabledReason);
            }
        }
    }
}
