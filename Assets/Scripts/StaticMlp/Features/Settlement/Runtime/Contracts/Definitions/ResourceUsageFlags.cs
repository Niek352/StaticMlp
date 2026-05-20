using System;

namespace StaticMlp.Features.Settlement
{
    [Flags]
    public enum ResourceUsageFlags : byte
    {
        None = 0,
        Construction = 1 << 0,
        ExpeditionReward = 1 << 1,
        Upkeep = 1 << 2,
        Fuel = 1 << 3,
        Repair = 1 << 4,
        ProductionInput = 1 << 5,
        ProductionOutput = 1 << 6,
        Progression = 1 << 7
    }
}
