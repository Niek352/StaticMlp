namespace StaticMlp.Features.Frontier
{
    public struct ThreatBannerState
    {
        public bool IsVisible;
        public ThreatPhase Phase;
        public RaidScheduleStatus RaidStatus;
        public uint ActivateAtTick;
    }
}
