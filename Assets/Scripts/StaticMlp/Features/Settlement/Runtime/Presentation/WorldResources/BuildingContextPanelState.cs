using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Buildings;
using Unity.Collections;

namespace StaticMlp.Features.Settlement
{
    public struct BuildingContextPanelState
    {
        public SettlementAnchorId AnchorId;
        public EntityGID FocusedSite;
        public bool HasFocusedSite;
        public string BuildingDisplayName;
        public BuildingAvailableActionPresentation PrimaryBuildingAction;
        public BuildingAvailableActionPresentation SecondaryBuildingAction;
        public bool HasOpenedBuildingAction;
        public BuildingInteractionKind OpenedBuildingActionKind;
        public string OpenedBuildingActionLabel;
        public string OpenedBuildingActionSummary;
        public ConstructionPhase ConstructionPhase;
        public FixedList512Bytes<ConstructionResourceViewEntry> ConstructionResources;
        public float Progress01;
        public bool CanDepositResources;
        public bool CanBuild;
    }
}
