using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public static class BedSlotRules
    {
        public static BedSlotState Reserve(in BedSlotState slot, EntityGID worker)
        {
            if (slot.Status == BedSlotStatus.Blocked)
                throw new InvalidOperationException($"Bed slot {slot.SlotIndex} is blocked and cannot be reserved.");

            if (slot.Status != BedSlotStatus.Free)
                throw new InvalidOperationException($"Bed slot {slot.SlotIndex} is not free.");

            return new BedSlotState(slot.SlotIndex, BedSlotStatus.Reserved, worker);
        }

        public static BedSlotState Occupy(in BedSlotState slot, EntityGID worker)
        {
            if (slot.Status != BedSlotStatus.Reserved)
                throw new InvalidOperationException($"Bed slot {slot.SlotIndex} must be reserved before it can be occupied.");

            if (slot.Occupant != worker)
                throw new InvalidOperationException($"Bed slot {slot.SlotIndex} is reserved by a different worker.");

            return new BedSlotState(slot.SlotIndex, BedSlotStatus.Occupied, worker);
        }

        public static BedSlotState Release(in BedSlotState slot)
        {
            if (slot.Status == BedSlotStatus.Blocked)
                throw new InvalidOperationException($"Bed slot {slot.SlotIndex} is blocked and cannot be released.");

            return new BedSlotState(slot.SlotIndex, BedSlotStatus.Free);
        }

        public static BedSlotState Block(in BedSlotState slot)
        {
            return new BedSlotState(slot.SlotIndex, BedSlotStatus.Blocked);
        }
    }
}
