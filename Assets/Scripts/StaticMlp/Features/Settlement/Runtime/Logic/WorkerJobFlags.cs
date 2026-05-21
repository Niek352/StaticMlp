using System;

namespace StaticMlp.Features.Settlement
{
    [Flags]
    public enum WorkerJobFlags : byte
    {
        None = 0,
        DeliverConstructionResources = 1 << 0,
        BuildConstruction = 1 << 1,
        GatherResources = 1 << 2,
        HaulResources = 1 << 3,
        ProcessRecipe = 1 << 4,
        GuardPost = 1 << 5,
        MaintainBuildings = 1 << 6
    }
}
