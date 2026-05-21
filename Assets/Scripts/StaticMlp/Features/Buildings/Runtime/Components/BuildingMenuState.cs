using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;

namespace StaticMlp.Features.Buildings
{
    public struct BuildingMenuState : IResource
    {
        public bool IsOpen;
        public BuildingId SelectedBuildingId;

        public bool HasSelection => SelectedBuildingId != default;

        public void Select(BuildingId buildingId)
        {
            IsOpen = false;
            SelectedBuildingId = buildingId;
        }

        public void ClearSelection()
        {
            SelectedBuildingId = default;
        }
    }
}
