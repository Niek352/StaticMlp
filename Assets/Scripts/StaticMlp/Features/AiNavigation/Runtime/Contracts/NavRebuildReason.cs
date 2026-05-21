namespace StaticMlp.Features.AiNavigation
{
    public enum NavRebuildReason : byte
    {
        InitialBuild = 1,
        NavAreaChanged = 2,
        SourceGeometryChanged = 3,
        Prewarm = 4
    }
}
