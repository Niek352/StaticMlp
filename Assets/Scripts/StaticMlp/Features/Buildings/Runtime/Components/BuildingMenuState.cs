using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Buildings
{
    public struct BuildingMenuState : IResource
    {
        public bool IsOpen;
        public bool HasSelection;
        public ushort SelectedBuildingId;
        public int SelectionFrame;

        public void Select(ushort buildingId, int selectionFrame)
        {
            IsOpen = false;
            HasSelection = true;
            SelectedBuildingId = buildingId;
            SelectionFrame = selectionFrame;
        }

        public void ClearSelection()
        {
            HasSelection = false;
            SelectedBuildingId = 0;
            SelectionFrame = -1;
        }
    }
}
