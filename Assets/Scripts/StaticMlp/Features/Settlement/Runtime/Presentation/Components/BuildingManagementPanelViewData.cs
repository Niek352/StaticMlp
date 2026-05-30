using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public struct BuildingManagementPanelViewData : IComponent, ITrackableAdded, ITrackableChanged
    {
        public BuildingPanelState State;
    }
}
