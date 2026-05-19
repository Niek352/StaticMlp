using System;
using Code.EcsUi.Mvc;
using Cysharp.Threading.Tasks;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Frontier;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class Stage1HudController
        : ControllerBase<Stage1HudView>, IResourcePresentationController<Stage1HudState>
    {
        private readonly IMvcManager _mvcManager;

        public Stage1HudController(
            IMvcManager mvcManager,
            ViewFactoryMethod<Stage1HudView> viewFactory,
            ControllerResourceBridgeSystem<Stage1HudController, Stage1HudState> bridge)
            : base(viewFactory)
        {
            _mvcManager = mvcManager ?? throw new ArgumentNullException(nameof(mvcManager));
            AddModule(new BridgeSystemBinding<ControllerResourceBridgeSystem<Stage1HudController, Stage1HudState>, Stage1HudController>(this, bridge));
        }

        public override ViewLayer Layer => ViewLayer.Persistent;
        public override int? PersistentSortOrder => 0;

        public static string DescribeObjective(Stage1ObjectiveKind objective)
        {
            return objective switch
            {
                Stage1ObjectiveKind.RepairCamp => "Repair the camp core",
                Stage1ObjectiveKind.AssignWorker => "Assign the camp builder",
                Stage1ObjectiveKind.PrepareBuild => "Choose the next combat build",
                Stage1ObjectiveKind.StartExpedition => "Start the nearby expedition",
                Stage1ObjectiveKind.ClearExpedition => "Clear the hostile expedition",
                Stage1ObjectiveKind.DefendCamp => "Defend the camp from the raid",
                Stage1ObjectiveKind.PrepareBoss => "Spend the reward on boss preparation",
                Stage1ObjectiveKind.StartBossEncounter => "Begin the boss encounter",
                Stage1ObjectiveKind.DefeatBoss => "Defeat the boss",
                Stage1ObjectiveKind.VerticalSliceComplete => "Vertical slice complete",
                _ => "Stabilize the camp",
            };
        }

        public static string DescribeBuild(LoadoutModuleId moduleId)
        {
            if (moduleId == LoadoutModuleCatalog.PoisonArrowModuleId)
                return "Poison Archer";

            if (moduleId == LoadoutModuleCatalog.FireFlaskModuleId)
                return "Fire Bomber";

            return "Not prepared";
        }

        public void Apply(in Stage1HudState state)
        {
            View.Render(in state);
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
            ref var session = ref CW.GetResource<Stage1HudSession>();
            session.IsVisible = false;
        }
    }
}
