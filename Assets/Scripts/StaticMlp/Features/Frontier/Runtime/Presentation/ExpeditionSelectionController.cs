using System;
using Code.EcsUi.Mvc;
using StaticMlp.Features.Build;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Frontier
{
    public sealed class ExpeditionSelectionController : ControllerBase<ExpeditionSelectionView>
    {
        public ExpeditionSelectionController(
            ViewFactoryMethod<ExpeditionSelectionView> viewFactory,
            ControllerResourceBridgeSystem<ExpeditionSelectionController, ExpeditionSelectionScreenState> bridge)
            : base(viewFactory)
        {
            AddModule(new BridgeSystemBinding<ControllerResourceBridgeSystem<ExpeditionSelectionController, ExpeditionSelectionScreenState>, ExpeditionSelectionController>(this, bridge));
        }

        public override ViewLayer Layer => ViewLayer.Fullscreen;

        public static string DescribePreparedBuild(BuildModuleId moduleId)
        {
            if (moduleId == BuildModuleCatalog.PoisonArrowModuleId)
                return "Poison Archer";

            if (moduleId == BuildModuleCatalog.FireFlaskModuleId)
                return "Fire Bomber";

            return "Not prepared";
        }

        public void Apply(in ExpeditionSelectionScreenState state)
        {
            View.Render(in state);
        }

        protected override void OnViewInstantiated()
        {
            View.Bind(StartExpedition, RequestClose);
        }

        public override void Dispose()
        {
            if (View != null)
                View.Unbind();

            base.Dispose();
        }

        private void StartExpedition()
        {
            ref readonly var state = ref CW.GetResource<ExpeditionSelectionScreenState>();
            var request = new StartExpeditionRequestEvent(state.AnchorId, state.ExpeditionId);
            if (!CW.SendToServerEvent(in request))
                throw new InvalidOperationException("Failed to enqueue Stage 1 expedition start request.");

            RequestClose();
        }
    }
}
