using System;
using Code.EcsUi.Mvc;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;
using UnityEngine;

namespace StaticMlp.Features.Settlement
{
    public sealed class Stage1ContextPanelController
        : ControllerBase<Stage1ContextPanelView>, IResourcePresentationController<Stage1ContextPanelState>
    {
        private const float UI_BUILD_WORK_PER_CLICK = 35f;

        public Stage1ContextPanelController(
            ViewFactoryMethod<Stage1ContextPanelView> viewFactory,
            ControllerResourceBridgeSystem<Stage1ContextPanelController, Stage1ContextPanelState> bridge)
            : base(viewFactory)
        {
            AddModule(new BridgeSystemBinding<ControllerResourceBridgeSystem<Stage1ContextPanelController, Stage1ContextPanelState>, Stage1ContextPanelController>(this, bridge));
        }

        public override ViewLayer Layer => ViewLayer.Persistent;
        public override int? PersistentSortOrder => 100;

        public void Apply(in Stage1ContextPanelState state)
        {
            View.Render(in state);
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

        private static void HandlePrimaryAction()
        {
            ref readonly var state = ref CW.GetResource<Stage1ContextPanelState>();
            if (state.Mode == Stage1ContextPanelMode.Building)
            {
                SendDepositOrBuild(state);
                return;
            }

            ToggleWorkerAssignment(state);
        }

        private static void HandleSecondaryAction()
        {
        }

        private static void SendDepositOrBuild(in Stage1ContextPanelState state)
        {
            if (state.CanDepositResources)
            {
                var depositRequest = new DepositConstructionResourcesRequestEvent(
                    state.FocusedSite,
                    Mathf.Max(0, state.WoodRequired - state.WoodDelivered),
                    Mathf.Max(0, state.StoneRequired - state.StoneDelivered));
                RequestApi.Send<DepositConstructionResourcesRequestEvent, DepositConstructionResourcesResultEvent>(depositRequest);
                return;
            }

            if (!state.CanBuild)
                return;

            var buildRequest = new BuildConstructionRequestEvent(state.FocusedSite, UI_BUILD_WORK_PER_CLICK);
            RequestApi.Send<BuildConstructionRequestEvent, BuildConstructionResultEvent>(buildRequest);
        }

        private static void ToggleWorkerAssignment(in Stage1ContextPanelState state)
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
