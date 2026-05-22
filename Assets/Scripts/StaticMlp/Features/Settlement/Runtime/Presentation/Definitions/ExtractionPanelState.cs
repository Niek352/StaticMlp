using FFS.Libraries.StaticEcs;
using Unity.Collections;

namespace StaticMlp.Features.Settlement
{
    public struct ExtractionPanelState
    {
        public EntityGID Target;
        public SettlementAnchorId AnchorId;
        public string DisplayName;
        public ResourceId OutputResource;
        public int BufferAmount;
        public int BufferCapacity;
        public byte WorkerSlotCount;
        public byte AssignedWorkerCount;
        public FixedList128Bytes<BuildingWorkerSlot> WorkerSlots;
    }
}
