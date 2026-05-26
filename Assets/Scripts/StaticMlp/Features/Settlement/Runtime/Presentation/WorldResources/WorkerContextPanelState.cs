using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Settlement.Workers;
using Unity.Collections;

namespace StaticMlp.Features.Settlement
{
    public struct WorkerContextPanelState
    {
        public SettlementAnchorId AnchorId;
        public EntityGID WorkerId;
        public bool HasWorker;
        public bool WorkerAssigned;
        public bool CanToggleWorkerAssignment;
        public AiTaskType WorkerActiveTask;
        public SettlementWorkerBlockingReason WorkerBlockingReason;
        public FixedList128Bytes<WorkerListEntryState> Workers;
    }
}
