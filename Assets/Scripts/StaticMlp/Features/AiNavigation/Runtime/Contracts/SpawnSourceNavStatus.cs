namespace StaticMlp.Features.AiNavigation
{
    public enum SpawnSourceNavStatus : byte
    {
        Unknown = 0,
        WaitingForNavMesh = 1,
        Reachable = 2,
        Unreachable = 3,
        TemporarilyBlocked = 4,
    }
}
