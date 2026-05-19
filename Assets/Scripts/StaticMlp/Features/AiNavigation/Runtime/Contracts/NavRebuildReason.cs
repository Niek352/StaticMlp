namespace StaticMlp.Features.AiNavigation
{
    public enum NavRebuildReason : byte
    {
        InitialBuild = 1,
        CombatCellNavAreaChanged = 2,
        Prewarm = 3
    }
}
