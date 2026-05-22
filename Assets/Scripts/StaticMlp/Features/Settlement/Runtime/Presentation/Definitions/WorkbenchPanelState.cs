using FFS.Libraries.StaticEcs;
using Unity.Collections;

namespace StaticMlp.Features.Settlement
{
    public struct WorkbenchPanelState
    {
        public EntityGID Target;
        public SettlementAnchorId AnchorId;
        public string DisplayName;
        public string RecipeName;
        public float WorkDone;
        public float WorkRequired;
        public byte WorkerSlotCount;
        public byte AssignedWorkerCount;
        public FixedList128Bytes<ResourceAmount> Inputs;
        public FixedList128Bytes<ResourceAmount> Outputs;
    }
}
