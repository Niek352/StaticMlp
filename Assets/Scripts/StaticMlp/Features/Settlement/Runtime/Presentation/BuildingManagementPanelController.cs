using System;
using Code.EcsUi.Mvc;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public sealed class BuildingManagementPanelController
        : ControllerBase<BuildingManagementPanelView>
    {
        private BuildingManagementPanelState _lastState;

        public BuildingManagementPanelController(
            ViewFactoryMethod<BuildingManagementPanelView> viewFactory,
            BuildingManagementPanelBridgeSystem bridge)
            : base(viewFactory)
        {
            AddModule(new BridgeSystemBinding<BuildingManagementPanelBridgeSystem, BuildingManagementPanelController>(this, bridge));
        }

        public override ViewLayer Layer => ViewLayer.Persistent;
        public override int? PersistentSortOrder => 100;

        public void Apply(in BuildingManagementPanelState state)
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
            HandleBuildingAction(_lastState.PrimaryBuildingAction);
        }

        private void HandleSecondaryAction()
        {
            HandleBuildingAction(_lastState.SecondaryBuildingAction);
        }

        private static void HandleBuildingAction(in BuildingAvailableActionPresentation action)
        {
            if (!action.IsDefined)
                throw new InvalidOperationException("Building management action requires a defined action.");

            if (!action.Enabled)
                throw new InvalidOperationException($"Building management action {action.Kind} is disabled: {action.DisabledReason}");

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
                    throw new InvalidOperationException($"Unsupported building management action {action.Kind}.");
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

        private static void OpenOperationIntent(EntityGID target, BuildingInteractionKind kind)
        {
            ref var intent = ref CW.GetResource<BuildingManagementOperationOpenIntent>();
            intent.Set(target, kind);
        }

        private static void HandleClose()
        {
            ref var session = ref CW.GetResource<BuildingManagementPanelSession>();
            session.Close();

            ref var intent = ref CW.GetResource<BuildingManagementOperationOpenIntent>();
            intent.Clear();
        }
    }
}
