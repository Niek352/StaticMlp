using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public struct BuildingPanelSession : IResource
    {
        public bool IsOpen;
        public BuildingPanelRoute Route;
        public bool OpenedFromInteraction;

        public EntityGID Target => Route.Target;
        public BuildingPanelKind Kind => Route.Kind;

        public void Open(BuildingPanelRoute route, bool openedFromInteraction)
        {
            IsOpen = true;
            Route = route;
            OpenedFromInteraction = openedFromInteraction;
        }

        public void Close()
        {
            IsOpen = false;
            Route = default;
            OpenedFromInteraction = false;
        }
    }
}
