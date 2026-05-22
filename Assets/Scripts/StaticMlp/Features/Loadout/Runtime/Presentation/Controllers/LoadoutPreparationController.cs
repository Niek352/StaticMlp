using System;
using Code.EcsUi.Mvc;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Features.Loadout
{
    public sealed class LoadoutPreparationController
        : ControllerBase<LoadoutPreparationView>
    {
        private LoadoutPreparationScreenState _lastState;

        public LoadoutPreparationController(
            ViewFactoryMethod<LoadoutPreparationView> viewFactory,
            LoadoutPreparationBridgeSystem bridge)
            : base(viewFactory)
        {
            AddModule(new BridgeSystemBinding<LoadoutPreparationBridgeSystem, LoadoutPreparationController>(this, bridge));
        }

        public override ViewLayer Layer => ViewLayer.Fullscreen;

        public static string DescribeModule(LoadoutModuleId moduleId)
        {
            if (moduleId == LoadoutModuleCatalog.PoisonArrowModuleId)
                return "Poison Archer";

            if (moduleId == LoadoutModuleCatalog.FireFlaskModuleId)
                return "Fire Bomber";

            return "None";
        }

        public void Apply(in LoadoutPreparationScreenState state)
        {
            _lastState = state;
            View.Render(in state);
        }

        protected override void OnViewInstantiated()
        {
            View.Bind(SelectPoisonArrow, SelectFireFlask, ConfirmBuild, RequestClose);
        }

        public override void Dispose()
        {
            if (View != null)
                View.Unbind();

            base.Dispose();
        }

        private void SelectPoisonArrow()
        {
            SelectModule(LoadoutModuleCatalog.PoisonArrowModuleId);
        }

        private void SelectFireFlask()
        {
            SelectModule(LoadoutModuleCatalog.FireFlaskModuleId);
        }

        private static void SelectModule(LoadoutModuleId moduleId)
        {
            foreach (var player in CW.Query<All<LocalOwned, PlayerTag, OwnerLoadoutSelection>>().Entities())
            {
                ref var selection = ref player.Mut<OwnerLoadoutSelection>();
                selection.PrimaryModuleId = moduleId;
                player.Set(Stage1LoadoutRules.CreatePreparedSnapshot(selection));
                return;
            }

            throw new InvalidOperationException("Build preparation requires a local player with owner build selection.");
        }

        private void ConfirmBuild()
        {
            if (_lastState.CanPrepareBoss)
            {
                var request = new PrepareBossRequestEvent(SettlementAnchorCatalog.HomeCampId);
                CW.SendToServer(in request);
                RequestClose();
                return;
            }

            foreach (var player in CW.Query<All<LocalOwned, PlayerTag, ClientLoadoutSelectionSyncState>>().Entities())
            {
                ref var syncState = ref player.Mut<ClientLoadoutSelectionSyncState>();
                syncState.ShouldCommitSelection = true;
                RequestClose();
                return;
            }

            throw new InvalidOperationException("Build preparation confirmation requires a local player sync state.");
        }
    }
}
