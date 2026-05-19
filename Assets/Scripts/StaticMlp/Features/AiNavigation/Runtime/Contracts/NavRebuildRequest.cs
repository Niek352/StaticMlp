using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.AiNavigation
{
    public struct NavRebuildRequest : IComponent
    {
        public NavRebuildReason Reason;
        public int Priority;
        public uint RequestedAtTick;
        public bool AllowDuringPeak;
    }
}
