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
        private BuildingPanelState _lastState;

        public BuildingManagementPanelController(
            ViewFactoryMethod<BuildingManagementPanelView> viewFactory,
            BuildingManagementPanelBridgeSystem bridge)
            : base(viewFactory)
        {
            AddModule(new BridgeSystemBinding<BuildingManagementPanelBridgeSystem, BuildingManagementPanelController>(this, bridge));
        }

        public override ViewLayer Layer => ViewLayer.Persistent;
        public override int? PersistentSortOrder => 100;

        public void Apply(in BuildingPanelState state)
        {
            _lastState = state;
            View.Render(in state);
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
            HandlePanelAction(_lastState.PrimaryAction);
        }

        private void HandleSecondaryAction()
        {
            HandlePanelAction(_lastState.SecondaryAction);
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
                default:
                    throw new InvalidOperationException($"Unsupported building panel action {action.Kind}.");
            }
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

        private static void HandleClose()
        {
            ref var session = ref CW.GetResource<BuildingPanelSession>();
            session.Close();
        }
    }
}
