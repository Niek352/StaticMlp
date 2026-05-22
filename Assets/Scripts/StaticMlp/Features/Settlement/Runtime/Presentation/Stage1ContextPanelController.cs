using System;
using Code.EcsUi.Mvc;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public sealed class Stage1ContextPanelController
        : ControllerBase<Stage1ContextPanelView>
    {
        private BuildingContextPanelState _lastBuildingState;
        private WorkerContextPanelState _lastWorkerState;

        public Stage1ContextPanelController(
            ViewFactoryMethod<Stage1ContextPanelView> viewFactory,
            Stage1ContextPanelCompositeBridgeSystem bridge)
            : base(viewFactory)
        {
            AddModule(new BridgeSystemBinding<Stage1ContextPanelCompositeBridgeSystem, Stage1ContextPanelController>(this, bridge));
        }

        public override ViewLayer Layer => ViewLayer.Persistent;
        public override int? PersistentSortOrder => 100;

        public void Apply(
            in BuildingContextPanelState building,
            in WorkerContextPanelState worker,
            Stage1ContextPanelMode mode)
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
            ref readonly var session = ref CW.GetResource<Stage1ContextPanelSession>();
            if (session.Mode == Stage1ContextPanelMode.Building)
            {
                HandleBuildingAction(_lastBuildingState.PrimaryBuildingAction);
                return;
            }

            ToggleWorkerAssignment(in _lastWorkerState);
        }

        private void HandleSecondaryAction()
        {
            ref readonly var session = ref CW.GetResource<Stage1ContextPanelSession>();
            if (session.Mode == Stage1ContextPanelMode.Building)
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

            switch (action.Kind)
            {
                case BuildingInteractionKind.DepositConstructionResources:
                    SendDeposit(action.Target);
                    return;
                case BuildingInteractionKind.ContributeBuildWork:
                    SendBuild(action.Target);
                    return;
                case BuildingInteractionKind.OpenDetails:
                case BuildingInteractionKind.AssignWorker:
                case BuildingInteractionKind.OpenProductionQueue:
                case BuildingInteractionKind.SetRecipe:
                case BuildingInteractionKind.ClaimOutput:
                case BuildingInteractionKind.AssignBed:
                case BuildingInteractionKind.ToggleEnabled:
                case BuildingInteractionKind.TriggerRepair:
                case BuildingInteractionKind.Extract:
                case BuildingInteractionKind.Rest:
                case BuildingInteractionKind.StoreItems:
                case BuildingInteractionKind.WithdrawItems:
                    OpenOperationIntent(action.Target, action.Kind);
                    return;
                default:
                    throw new InvalidOperationException($"Unsupported building context action {action.Kind}.");
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

        private static void OpenOperationIntent(EntityGID target, BuildingInteractionKind kind)
        {
            ref var intent = ref CW.GetResource<Stage1BuildingOperationOpenIntent>();
            intent.Set(target, kind);
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
