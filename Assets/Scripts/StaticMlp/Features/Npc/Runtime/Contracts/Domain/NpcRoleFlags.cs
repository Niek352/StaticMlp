using System;

namespace StaticMlp.Features.Npc
{
    [Flags]
    public enum NpcRoleFlags : ushort
    {
        None = 0,
        Gatherer = 1 << 0,
        Hauler = 1 << 1,
        Processor = 1 << 2,
        Guard = 1 << 3,
        Builder = 1 << 4,
        Researcher = 1 << 5
    }
}
