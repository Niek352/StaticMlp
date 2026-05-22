using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement.Workers;
using Unity.Collections;

namespace StaticMlp.Features.Settlement
{
    public struct SettlementHudState : IResource
    {
        public SettlementAnchorId AnchorId;
        public Stage1ObjectiveKind Objective;
        public string ObjectiveHint;
        public Stage1SettlementProgressStage SettlementStage;
        public FixedList512Bytes<SettlementResourceViewEntry> Resources;
        public ushort TotalWorkers;
        public ushort AssignedWorkers;
        public SettlementWorkerBlockingReason WorkerBlockingReason;
        public bool CanOpenLoadoutPreparation;
        public bool CanOpenExpeditionSelection;
    }
}
