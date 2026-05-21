namespace StaticMlp.Features.Settlement
{
    public enum Stage1SettlementProgressStage : byte
    {
        DamagedCampStart = 0,
        RepairObjectiveActive = 1,
        RepairResourcesReady = 2,
        CampRepaired = 3,
        WorkerAssigned = 4,
        StockpilePlaced = 5,
        ShelterPlaced = 6,
        ExtractionOnline = 7,
        WorkbenchOnline = 8,
        LoadoutPrepared = 9,
    }
}
