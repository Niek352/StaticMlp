using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public struct BuildingManagementPanelSession : IResource
    {
        public bool IsOpen;
        public EntityGID Target;
        public bool OpenedFromInteraction;

        public void Open(EntityGID target, bool openedFromInteraction)
        {
            IsOpen = true;
            Target = target;
            OpenedFromInteraction = openedFromInteraction;
        }

        public void Close()
        {
            IsOpen = false;
            Target = default;
            OpenedFromInteraction = false;
        }
    }
}
