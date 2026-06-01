using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public readonly struct BuildingManagementPanelActionIntent : IEvent
    {
        public readonly BuildingPanelAction Action;

        public BuildingManagementPanelActionIntent(BuildingPanelAction action)
        {
            Action = action;
        }
    }
}
