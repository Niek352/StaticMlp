using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    [Obsolete("Temp")]
    public partial struct WorkbenchOperationState : IComponent, ITrackableChanged
    {
        public ushort ActiveRecipeId;

        public bool Enabled;

        public byte WorkerSlotCount;

        public float WorkDone;
    }
}
