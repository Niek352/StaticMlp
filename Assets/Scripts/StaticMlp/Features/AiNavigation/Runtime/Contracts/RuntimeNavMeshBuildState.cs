namespace StaticMlp.Features.AiNavigation
{
    public enum RuntimeNavMeshBuildState : byte
    {
        None = 0,
        Queued = 1,
        Building = 2,
        Ready = 3
    }
}
