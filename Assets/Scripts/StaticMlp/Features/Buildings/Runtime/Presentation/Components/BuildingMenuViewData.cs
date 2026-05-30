using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Buildings
{
    public struct BuildingMenuViewData : IComponent, ITrackableAdded, ITrackableChanged
    {
        public BuildingMenuPresentation Presentation;
    }
}
