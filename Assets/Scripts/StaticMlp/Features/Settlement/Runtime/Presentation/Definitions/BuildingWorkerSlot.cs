using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public readonly struct BuildingWorkerSlot
    {
        public readonly byte SlotIndex;
        public readonly EntityGID Worker;
        public readonly bool Assigned;

        public BuildingWorkerSlot(byte slotIndex, EntityGID worker, bool assigned)
        {
            SlotIndex = slotIndex;
            Worker = worker;
            Assigned = assigned;
        }
    }
}
