namespace StaticMlp.Features.Settlement
{
    public enum BuildingPanelActionKind : byte
    {
        None = 0,
        DepositConstructionResources = 1,
        ContributeBuildWork = 2,
        AssignWorker = 3,
        UnassignWorker = 4,
        CollectExtractionOutput = 5,
        DepositCarriedResourcesToStockpile = 6
    }
}
