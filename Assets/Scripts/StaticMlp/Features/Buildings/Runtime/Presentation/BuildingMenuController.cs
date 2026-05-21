using System;
using Code.EcsUi.Mvc;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    public sealed class BuildingMenuController : ControllerBase<BuildingMenuView>
    {
        public BuildingMenuController(ViewFactoryMethod<BuildingMenuView> viewFactory)
            : base(viewFactory)
        {
        }

        public override ViewLayer Layer => ViewLayer.Persistent;

        public override int? PersistentSortOrder => 80;

        public void SyncPresentation()
        {
            if (View == null)
                throw new InvalidOperationException(
                    $"{nameof(BuildingMenuController)} cannot sync presentation before its view is created and shown.");

            ref readonly var state = ref CW.GetResource<BuildingMenuState>();
            View.Render(BuildPresentation(in state));
        }

        protected override void OnViewInstantiated()
        {
            View.Bind(SelectBuilding, CloseMenu);
        }

        protected override void OnBeforeViewShow()
        {
            SyncPresentation();
        }

        public override void Dispose()
        {
            if (View != null)
                View.Unbind();

            base.Dispose();
        }

        private static BuildingMenuPresentation BuildPresentation(in BuildingMenuState state)
        {
            return BuildingMenuPresentation.Create(in state);
        }

        private static void CloseMenu()
        {
            ref var state = ref CW.GetResource<BuildingMenuState>();
            state.IsOpen = false;
            state.ClearSelection();
        }

        private static void SelectBuilding(BuildingId buildingId)
        {
            ref var state = ref CW.GetResource<BuildingMenuState>();
            BuildingCatalogData.Get(buildingId);
            state.Select(buildingId);
        }
    }
}
