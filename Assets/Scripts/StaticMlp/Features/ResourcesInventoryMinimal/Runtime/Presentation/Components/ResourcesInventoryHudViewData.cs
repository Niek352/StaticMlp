using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public struct ResourcesInventoryHudViewData : IComponent, ITrackableAdded, ITrackableChanged
    {
        public ResourcesInventoryHudPresentation Presentation;
    }
}
