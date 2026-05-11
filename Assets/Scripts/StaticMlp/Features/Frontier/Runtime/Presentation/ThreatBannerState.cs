using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Frontier
{
    public struct ThreatBannerState : IResource
    {
        public bool IsVisible;
        public ThreatPhase Phase;
        public RaidScheduleStatus RaidStatus;
        public uint ActivateAtTick;
    }
}
