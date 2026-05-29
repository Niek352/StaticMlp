using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Settlement
{
    public struct BuildingPanelState
    {
        public bool IsOpen;
        public EntityGID Target;
        public BuildingPanelKind Kind;
        public string Title;
        public string TransferFeedbackMessage;
        public BuildingPanelAction PrimaryAction;
        public BuildingPanelAction SecondaryAction;
        public ConstructionPanelState Construction;
        public StockpilePanelState Stockpile;
        public ExtractionPanelState Extraction;
        public WorkbenchPanelState Workbench;
        public ShelterPanelState Shelter;
        public CampCorePanelState CampCore;
    }
}
