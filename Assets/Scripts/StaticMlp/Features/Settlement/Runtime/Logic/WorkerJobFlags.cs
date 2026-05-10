using System;

namespace StaticMlp.Features.Settlement
{
    [Flags]
    public enum WorkerJobFlags : byte
    {
        None = 0,
        DeliverConstructionResources = 1 << 0,
        BuildConstruction = 1 << 1,
    }
}
