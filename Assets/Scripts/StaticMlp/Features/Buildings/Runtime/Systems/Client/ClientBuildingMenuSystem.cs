using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;

namespace StaticMlp.Features.Buildings
{
    public sealed class ClientBuildingMenuSystem : ISystem
    {
        public void Update()
        {
            var entity = BuildingMenuStateUtility.GetOrCreate();
            ref var state = ref entity.Mut<BuildingMenuState>();

            if (BuildingPlacementInput.ToggleMenuWasPressedProvider())
                state.IsOpen = !state.IsOpen;

            BuildingMenuCommands.Consume(
                out var toggleRequested,
                out var openRequested,
                out var closeRequested,
                out var selectRequested,
                out var selectedBuildingId);

            if (toggleRequested)
                state.IsOpen = !state.IsOpen;

            if (openRequested)
                state.IsOpen = true;

            if (closeRequested)
            {
                state.IsOpen = false;
                state.ClearSelection();
            }

            var selectedThisFrame = false;
            if (selectRequested
                && StaticMlp.Features.BuildingCatalog.BuildingCatalog.TryGetDefinition(
                    new BuildingId(selectedBuildingId),
                    out _))
            {
                state.Select(selectedBuildingId);
                selectedThisFrame = true;
            }

            BuildingMenuRuntime.Publish(in state, selectedThisFrame);
        }
    }
}
