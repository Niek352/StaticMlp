using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement.Workers;

namespace StaticMlp.Features.Settlement
{
    public struct WorkerListEntryState
    {
        public EntityGID WorkerId;
        public WorkerRoleId Role;
        public bool IsAssigned;
        public bool IsBuildingAssignment;
        public EntityGID Building;
        public byte SlotIndex;
        public SettlementWorkerBlockingReason BlockingReason;
    }
}
