using System;
using Code.EcsUi.Mvc;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Networking;
using UnityEngine;

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
            View.Bind(SelectWoodenHut, CloseMenu);
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
            var definition = state.HasSelection
                ? BuildingCatalogData.GetDefinition(new BuildingId(state.SelectedBuildingId))
                : BuildingCatalogData.GetDefinition(BuildingCatalogData.WoodenHutId);

            return new BuildingMenuPresentation(
                state.IsOpen,
                definition.DisplayName,
                definition.CostWood,
                definition.CostStone);
        }

        private static void CloseMenu()
        {
            ref var state = ref CW.GetResource<BuildingMenuState>();
            state.IsOpen = false;
            state.ClearSelection();
        }

        private static void SelectWoodenHut()
        {
            ref var state = ref CW.GetResource<BuildingMenuState>();
            state.Select(BuildingCatalogData.WoodenHutId.Value, Time.frameCount);
        }
    }
}
