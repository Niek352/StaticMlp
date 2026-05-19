using System;
using Code.EcsUi.Mvc;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Frontier
{
    public sealed class ExpeditionSelectionController
        : ControllerBase<ExpeditionSelectionView>, IResourcePresentationController<ExpeditionSelectionScreenState>
    {
        public ExpeditionSelectionController(
            ViewFactoryMethod<ExpeditionSelectionView> viewFactory,
            ControllerResourceBridgeSystem<ExpeditionSelectionController, ExpeditionSelectionScreenState> bridge)
            : base(viewFactory)
        {
            AddModule(new BridgeSystemBinding<ControllerResourceBridgeSystem<ExpeditionSelectionController, ExpeditionSelectionScreenState>, ExpeditionSelectionController>(this, bridge));
        }

        public override ViewLayer Layer => ViewLayer.Fullscreen;

        public static string DescribePreparedBuild(LoadoutModuleId moduleId)
        {
            if (moduleId == LoadoutModuleCatalog.PoisonArrowModuleId)
                return "Poison Archer";

            if (moduleId == LoadoutModuleCatalog.FireFlaskModuleId)
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
            if (state.IsBossEncounterMode)
            {
                var bossRequest = new StartBossEncounterRequestEvent(state.AnchorId, state.BossId);
                CW.SendToServer(in bossRequest);
                RequestClose();
                return;
            }

            var request = new StartExpeditionRequestEvent(state.AnchorId, state.ExpeditionId);
            CW.SendToServer(in request);
            RequestClose();
        }
    }
}
