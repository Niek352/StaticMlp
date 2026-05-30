using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement.Workers;

namespace StaticMlp.Features.Settlement
{
    public struct SettlementContextPanelViewData : IComponent, ITrackableAdded, ITrackableChanged
    {
        public BuildingContextPanelState Building;
        public WorkerContextPanelState Worker;
        public SettlementContextPanelMode Mode;
    }
}
