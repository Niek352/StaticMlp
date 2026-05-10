using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;

namespace StaticMlp.Features.Buildings
{
    public struct BuildingMenuState : IResource
    {
        public bool IsOpen;
        public bool HasSelection;
        public BuildingId SelectedBuildingId;
        public int SelectionFrame;

        public void Select(BuildingId buildingId, int selectionFrame)
        {
            IsOpen = false;
            HasSelection = true;
            SelectedBuildingId = buildingId;
            SelectionFrame = selectionFrame;
        }

        public void ClearSelection()
        {
            HasSelection = false;
            SelectedBuildingId = default;
            SelectionFrame = -1;
        }
    }
}
