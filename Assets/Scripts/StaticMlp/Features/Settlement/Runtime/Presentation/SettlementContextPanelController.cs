using System;
using Code.EcsUi.Mvc;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementContextPanelController
        : ControllerBase<SettlementContextPanelView>
    {
        private BuildingContextPanelState _lastBuildingState;
        private WorkerContextPanelState _lastWorkerState;

        public SettlementContextPanelController(
            ViewFactoryMethod<SettlementContextPanelView> viewFactory,
            SettlementContextPanelCompositeBridgeSystem bridge)
            : base(viewFactory)
        {
            AddModule(new BridgeSystemBinding<SettlementContextPanelCompositeBridgeSystem, SettlementContextPanelController>(this, bridge));
        }

        public override ViewLayer Layer => ViewLayer.Persistent;
        public override int? PersistentSortOrder => 100;

        public void Apply(
            in BuildingContextPanelState building,
            in WorkerContextPanelState worker,
            SettlementContextPanelMode mode)
        {
            _lastBuildingState = building;
            _lastWorkerState = worker;
            View.Render(in building, in worker, mode);
        }

        protected override void OnViewInstantiated()
        {
            View.Bind(HandlePrimaryAction, HandleSecondaryAction);
        }

        public override void Dispose()
        {
            if (View != null)
                View.Unbind();

            base.Dispose();
        }

        private void HandlePrimaryAction()
        {
            ref readonly var session = ref CW.GetResource<SettlementContextPanelSession>();
            if (session.Mode == SettlementContextPanelMode.Building)
            {
                HandleBuildingAction(_lastBuildingState.PrimaryBuildingAction);
                return;
            }

            ToggleWorkerAssignment(in _lastWorkerState);
        }

        private void HandleSecondaryAction()
        {
            ref readonly var session = ref CW.GetResource<SettlementContextPanelSession>();
            if (session.Mode == SettlementContextPanelMode.Building)
            {
                HandleBuildingAction(_lastBuildingState.SecondaryBuildingAction);
            }
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
