using System;
using Aspid.StaticEcs.Windows;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementContextPanelViewModel : EcsWindowViewModelBase
    {
        public BuildingContextPanelState Building { get; private set; }
        public WorkerContextPanelState Worker { get; private set; }
        public SettlementContextPanelMode Mode { get; private set; }

        public void Sync(
            in BuildingContextPanelState building,
            in WorkerContextPanelState worker,
            SettlementContextPanelMode mode)
        {
            Building = building;
            Worker = worker;
            Mode = mode;
            NotifyChanged();
        }

        public void HandlePrimaryAction()
        {
            if (Mode == SettlementContextPanelMode.Building)
            {
                HandleBuildingAction(Building.PrimaryBuildingAction);
                return;
            }

            ToggleWorkerAssignment(in Worker);
        }

        public void HandleSecondaryAction()
        {
            if (Mode == SettlementContextPanelMode.Building)
                HandleBuildingAction(Building.SecondaryBuildingAction);
        }

        private static void HandleBuildingAction(in BuildingAvailableActionPresentation action)
        {
            if (!action.IsDefined)
                throw new InvalidOperationException("Building context action requires a defined action.");

            if (!action.Enabled)
                throw new InvalidOperationException($"Building context action {action.Kind} is disabled: {action.DisabledReason}");

            BuildingInteractionHandlerRegistry.Get(action.Kind).Execute(action.Target, action.Kind);
        }

        private static void ToggleWorkerAssignment(in WorkerContextPanelState state)
        {
            if (!state.HasWorker)
                throw new InvalidOperationException("Worker context action requires a replicated worker entity.");

            var request = new SetSettlementWorkerAssignmentRequestEvent(
                state.WorkerId,
                state.AnchorId,
                assigned: !state.WorkerAssigned);
            RequestApi.Send<SetSettlementWorkerAssignmentRequestEvent, SetSettlementWorkerAssignmentResultEvent>(request);
        }
    }
}
