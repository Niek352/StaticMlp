using System;

namespace StaticMlp.Features.Settlement
{
    [Flags]
    public enum ResourceUsageFlags : byte
    {
        None = 0,
        Construction = 1 << 0,
        ExpeditionReward = 1 << 1,
    }
}
