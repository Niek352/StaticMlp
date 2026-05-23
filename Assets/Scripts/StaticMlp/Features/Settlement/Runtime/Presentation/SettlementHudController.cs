using System;
using Code.EcsUi.Mvc;
using Cysharp.Threading.Tasks;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Progression;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementHudController
        : ControllerBase<SettlementHudView>
    {
        private readonly IMvcManager _mvcManager;

        public SettlementHudController(
            IMvcManager mvcManager,
            ViewFactoryMethod<SettlementHudView> viewFactory,
            SettlementHudCompositeBridgeSystem bridge)
            : base(viewFactory)
        {
            _mvcManager = mvcManager ?? throw new ArgumentNullException(nameof(mvcManager));
            AddModule(new BridgeSystemBinding<SettlementHudCompositeBridgeSystem, SettlementHudController>(this, bridge));
        }

        public override ViewLayer Layer => ViewLayer.Persistent;
        public override int? PersistentSortOrder => 0;

        public static string DescribeBuild(LoadoutModuleId moduleId)
        {
            if (moduleId == LoadoutModuleCatalog.PoisonArrowModuleId)
                return "Poison Archer";

            if (moduleId == LoadoutModuleCatalog.FireFlaskModuleId)
                return "Fire Bomber";

            return "Not prepared";
        }

        public void Apply(
            in SettlementHudState settlement,
            in ExpeditionHudState expedition,
            in LoadoutHudState loadout,
            in ThreatHudState threat,
            in RaidHudState raid,
            in BossHudState boss,
            in ProgressionHudState progression)
        {
            View.Render(in settlement, in expedition, in loadout, in threat, in raid, in boss, in progression);
        }

        protected override void OnViewInstantiated()
        {
            View.Bind(OpenLoadoutPreparation, OpenExpeditionSelection, CloseHud);
        }

        public override void Dispose()
        {
            if (View != null)
                View.Unbind();

            base.Dispose();
        }

        private void OpenLoadoutPreparation()
        {
            _mvcManager.ShowAsync(LoadoutPreparationController.IssueCommand()).Forget();
        }

        private void OpenExpeditionSelection()
        {
            _mvcManager.ShowAsync(ExpeditionSelectionController.IssueCommand()).Forget();
        }

        private static void CloseHud()
        {
            ref var session = ref CW.GetResource<SettlementHudSession>();
            session.IsVisible = false;
        }
    }
}
