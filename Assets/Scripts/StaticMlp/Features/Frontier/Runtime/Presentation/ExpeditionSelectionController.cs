using System;
using Code.EcsUi.Mvc;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Frontier
{
    public sealed class ExpeditionSelectionController
        : ControllerBase<ExpeditionSelectionView>
    {
        private ExpeditionSelectionScreenState _lastState;

        public ExpeditionSelectionController(
            ViewFactoryMethod<ExpeditionSelectionView> viewFactory,
            ExpeditionSelectionBridgeSystem bridge)
            : base(viewFactory)
        {
            AddModule(new BridgeSystemBinding<ExpeditionSelectionBridgeSystem, ExpeditionSelectionController>(this, bridge));
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
            _lastState = state;
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
            if (_lastState.IsBossEncounterMode)
            {
                var bossRequest = new StartBossEncounterRequestEvent(_lastState.AnchorId, _lastState.BossId);
                CW.SendToServer(in bossRequest);
                RequestClose();
                return;
            }

            var request = new StartExpeditionRequestEvent(_lastState.AnchorId, _lastState.ExpeditionId);
            CW.SendToServer(in request);
            RequestClose();
        }
    }
}
