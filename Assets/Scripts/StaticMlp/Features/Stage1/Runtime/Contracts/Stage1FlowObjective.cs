namespace StaticMlp.Features.Settlement
{
    public enum Stage1FlowObjective : byte
    {
        None = 0,
        RepairCamp = 1,
        AssignWorker = 2,
        PlaceStockpile = 3,
        PlaceShelter = 4,
        BringExtractionOnline = 5,
        BringWorkbenchOnline = 6,
        PrepareBuild = 7,
        StartExpedition = 8,
        ClearExpedition = 9,
        DefendCamp = 10,
        PrepareBoss = 11,
        StartBossEncounter = 12,
        DefeatBoss = 13,
        VerticalSliceComplete = 14,
    }
}
