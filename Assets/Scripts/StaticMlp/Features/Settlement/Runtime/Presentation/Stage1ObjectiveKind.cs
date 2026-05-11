namespace StaticMlp.Features.Settlement
{
    public enum Stage1ObjectiveKind : byte
    {
        None = 0,
        RepairCamp = 1,
        AssignWorker = 2,
        PrepareBuild = 3,
        StartExpedition = 4,
        ClearExpedition = 5,
        DefendCamp = 6,
        PrepareBoss = 7,
        StartBossEncounter = 8,
        DefeatBoss = 9,
        VerticalSliceComplete = 10,
    }
}
