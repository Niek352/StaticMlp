namespace StaticMlp.Features.BuildingCatalog
{
    public enum BuildingInteractionKind : byte
    {
        None = 0,
        OpenDetails = 1,
        DepositConstructionResources = 2,
        ContributeBuildWork = 3,
        AssignWorker = 4,
        OpenProductionQueue = 5,
        SetRecipe = 6,
        ClaimOutput = 7,
        AssignBed = 8,
        ToggleEnabled = 9,
        TriggerRepair = 10,
        Extract = 11,
        Rest = 12,
        StoreItems = 13,
        WithdrawItems = 14
    }
}
