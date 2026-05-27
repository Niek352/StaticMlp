using FFS.Libraries.StaticEcs;
using Unity.Collections;

namespace StaticMlp.Features.Settlement
{
    public struct WorkbenchPanelState
    {
        public EntityGID Target;
        public SettlementAnchorId AnchorId;
        public string DisplayName;
        public bool Enabled;
        public string RecipeName;
        public float WorkDone;
        public float WorkRequired;
        public int OutputAmount;
        public int OutputCapacity;
        public byte WorkerSlotCount;
        public byte AssignedWorkerCount;
        public FixedList128Bytes<BuildingWorkerSlot> WorkerSlots;
        public FixedList128Bytes<ProductionResourceBufferEntry> Inputs;
        public FixedList128Bytes<ProductionResourceBufferEntry> Outputs;
    }
}
