using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public sealed class ClientSettlementContextPanelActionIntentSystem : ISystem
    {
        private EventReceiver<ClientCoreWT, SettlementContextPanelActionIntent> _actionIntents;

        public void Init()
        {
            _actionIntents = CW.RegisterEventReceiver<SettlementContextPanelActionIntent>();
        }

        public void Destroy()
        {
            CW.DeleteEventReceiver(ref _actionIntents);
        }

        public void Update()
        {
            foreach (var evt in _actionIntents)
                HandleAction(evt.Value.IsPrimary);
        }

        private static void HandleAction(bool isPrimary)
        {
            var data = ReadViewData();
            if (data.Mode == SettlementContextPanelMode.Building)
            {
                HandleBuildingAction(isPrimary
                    ? data.Building.PrimaryBuildingAction
                    : data.Building.SecondaryBuildingAction);
                return;
            }

            if (!isPrimary)
                throw new InvalidOperationException("Worker context panel has no secondary action.");

            ToggleWorkerAssignment(in data.Worker);
        }

        private static SettlementContextPanelViewData ReadViewData()
        {
            foreach (var entity in CW.Query<All<SettlementContextPanelViewData>>().Entities())
                return entity.Read<SettlementContextPanelViewData>();

            throw new InvalidOperationException($"{nameof(SettlementContextPanelViewData)} entity is missing.");
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
