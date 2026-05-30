using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Frontier
{
    public struct ExpeditionSelectionViewData : IComponent, ITrackableAdded, ITrackableChanged
    {
        public ExpeditionSelectionScreenState State;
    }
}
