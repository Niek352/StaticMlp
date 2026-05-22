using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Frontier
{
    public struct RaidHudState : IResource
    {
        public RaidScheduleStatus Status;
        public uint ActivateAtTick;
    }
}
