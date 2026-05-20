using System;

namespace StaticMlp.Features.BuildingCatalog
{
    [Flags]
    public enum BuildingCapabilityFlags : ushort
    {
        None = 0,
        ProvidesHousing = 1 << 0,
        ProvidesStorage = 1 << 1,
        ProvidesWorkplace = 1 << 2,
        ProducesResources = 1 << 3,
        ConsumesResources = 1 << 4,
        ExtractsFromNode = 1 << 5,
        SupportsNpcInteraction = 1 << 6,
        SupportsPlayerInteraction = 1 << 7,
        RequiresMaintenance = 1 << 8,
        RequiresFuel = 1 << 9,
        ProvidesRest = 1 << 10,
        BlocksPathing = 1 << 11,
        OpensQueue = 1 << 12
    }
}
