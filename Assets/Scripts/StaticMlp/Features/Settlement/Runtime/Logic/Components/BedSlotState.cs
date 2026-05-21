using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public struct BedSlotState : IMultiComponent
    {
        public byte SlotIndex;
        public BedSlotStatus Status;
        public EntityGID Occupant;

        public BedSlotState(byte slotIndex, BedSlotStatus status, EntityGID occupant = default)
        {
            SlotIndex = slotIndex;
            Status = status;
            Occupant = occupant;
        }
    }
}
