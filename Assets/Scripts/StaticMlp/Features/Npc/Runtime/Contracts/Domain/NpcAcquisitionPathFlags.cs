using System;

namespace StaticMlp.Features.Npc
{
    [Flags]
    public enum NpcAcquisitionPathFlags : byte
    {
        None = 0,
        Extraction = 1 << 0,
        Rescue = 1 << 1,
        Incubation = 1 << 2,
        Seeded = 1 << 3
    }
}
