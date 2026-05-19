namespace StaticMlp.Features.AiNavigation
{
    public enum AiNavigationMode : byte
    {
        None = 0,
        GlobalRoute = 1,
        ApproximateMove = 2,
        WaitingForLocalNavMesh = 3,
        LocalNavMesh = 4,
        Stuck = 5,
    }
}
