namespace StaticMlp.Features.Settlement
{
    public enum Stage1SettlementProgressStage : byte
    {
        DamagedCampStart = 0,
        RepairObjectiveActive = 1,
        RepairResourcesReady = 2,
        CampRepaired = 3,
        WorkerAssigned = 4,
        LoadoutPrepared = 5,
    }
}
