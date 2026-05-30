using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Frontier
{
    public struct ThreatBannerViewData : IComponent, ITrackableAdded, ITrackableChanged
    {
        public ThreatBannerState State;
    }
}
