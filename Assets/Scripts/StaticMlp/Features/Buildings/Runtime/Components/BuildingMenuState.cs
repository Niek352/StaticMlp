using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Buildings
{
    public struct BuildingMenuState : IComponent, ITrackableChanged
    {
        public bool IsOpen;
        public bool HasSelection;
        public ushort SelectedBuildingId;

        public void Select(ushort buildingId)
        {
            IsOpen = false;
            HasSelection = true;
            SelectedBuildingId = buildingId;
        }

        public void ClearSelection()
        {
            HasSelection = false;
            SelectedBuildingId = 0;
        }
    }
}
