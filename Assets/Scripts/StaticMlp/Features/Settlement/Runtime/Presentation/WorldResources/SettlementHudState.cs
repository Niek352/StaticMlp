using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Features.CampFlow;
using Unity.Collections;

namespace StaticMlp.Features.Settlement
{
    public struct SettlementHudState
    {
        public SettlementAnchorId AnchorId;
        public string ObjectiveDisplayName;
        public string HintDisplayName;
        public CampFlowStage SettlementStage;
        public FixedList512Bytes<SettlementResourceViewEntry> Resources;
        public ushort TotalWorkers;
        public ushort AssignedWorkers;
        public SettlementWorkerBlockingReason WorkerBlockingReason;
        public bool CanOpenLoadoutPreparation;
        public bool CanOpenExpeditionSelection;
        public HudActionPresentation LoadoutPreparationAction;
        public HudActionPresentation ExpeditionSelectionAction;
    }
}
