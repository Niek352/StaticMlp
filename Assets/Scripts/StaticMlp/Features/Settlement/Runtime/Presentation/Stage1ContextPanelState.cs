using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using Unity.Collections;

namespace StaticMlp.Features.Settlement
{
    public struct Stage1ContextPanelState : IResource
    {
        public Stage1ContextPanelMode Mode;
        public SettlementAnchorId AnchorId;
        public EntityGID FocusedSite;
        public EntityGID WorkerId;
        public bool HasFocusedSite;
        public string BuildingDisplayName;
        public BuildingAvailableActionPresentation PrimaryBuildingAction;
        public BuildingAvailableActionPresentation SecondaryBuildingAction;
        public bool HasOpenedBuildingAction;
        public BuildingInteractionKind OpenedBuildingActionKind;
        public string OpenedBuildingActionLabel;
        public string OpenedBuildingActionSummary;
        public bool HasWorker;
        public ConstructionPhase ConstructionPhase;
        public FixedList512Bytes<ConstructionResourceViewEntry> ConstructionResources;
        public float Progress01;
        public bool CanDepositResources;
        public bool CanBuild;
        public bool WorkerAssigned;
        public bool CanToggleWorkerAssignment;
        public AiTaskType WorkerActiveTask;
        public SettlementWorkerBlockingReason WorkerBlockingReason;
    }
}
