using FFS.Libraries.StaticEcs;
using StaticMlp.Features.AiBots;
using StaticMlp.Features.Buildings;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;

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
        public bool HasWorker;
        public ConstructionPhase ConstructionPhase;
        public int WoodRequired;
        public int StoneRequired;
        public int WoodDelivered;
        public int StoneDelivered;
        public float Progress01;
        public bool CanDepositResources;
        public bool CanBuild;
        public bool WorkerAssigned;
        public bool CanToggleWorkerAssignment;
        public AiTaskType WorkerActiveTask;
        public SettlementWorkerBlockingReason WorkerBlockingReason;
    }
}
