using System;
using Code.EcsUi.Mvc;
using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Features.Build
{
    public sealed class BuildPreparationController
        : ControllerBase<BuildPreparationView>, IResourcePresentationController<BuildPreparationScreenState>
    {
        public BuildPreparationController(
            ViewFactoryMethod<BuildPreparationView> viewFactory,
            ControllerResourceBridgeSystem<BuildPreparationController, BuildPreparationScreenState> bridge)
            : base(viewFactory)
        {
            AddModule(new BridgeSystemBinding<ControllerResourceBridgeSystem<BuildPreparationController, BuildPreparationScreenState>, BuildPreparationController>(this, bridge));
        }

        public override ViewLayer Layer => ViewLayer.Fullscreen;

        public static string DescribeModule(BuildModuleId moduleId)
        {
            if (moduleId == BuildModuleCatalog.PoisonArrowModuleId)
                return "Poison Archer";

            if (moduleId == BuildModuleCatalog.FireFlaskModuleId)
                return "Fire Bomber";

            return "None";
        }

        public void Apply(in BuildPreparationScreenState state)
        {
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
            SelectModule(BuildModuleCatalog.PoisonArrowModuleId);
        }

        private void SelectFireFlask()
        {
            SelectModule(BuildModuleCatalog.FireFlaskModuleId);
        }

        private static void SelectModule(BuildModuleId moduleId)
        {
            foreach (var player in CW.Query<All<LocalOwned, PlayerTag, OwnerBuildSelection>>().Entities())
            {
                ref var selection = ref player.Mut<OwnerBuildSelection>();
                selection.PrimaryModuleId = moduleId;
                player.Set(Stage1BuildRules.CreatePreparedSnapshot(selection));
                return;
            }

            throw new InvalidOperationException("Build preparation requires a local player with owner build selection.");
        }

        private void ConfirmBuild()
        {
            foreach (var player in CW.Query<All<LocalOwned, PlayerTag, ClientBuildSelectionSyncState>>().Entities())
            {
                ref var syncState = ref player.Mut<ClientBuildSelectionSyncState>();
                syncState.ShouldCommitSelection = true;
                RequestClose();
                return;
            }

            throw new InvalidOperationException("Build preparation confirmation requires a local player sync state.");
        }
    }
}
