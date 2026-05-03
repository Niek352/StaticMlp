namespace StaticMlp.Game.Components.Buildings
{
    public enum ConstructionPhase : byte
    {
        WaitingForResources = 0,
        ReadyToBuild = 1,
        BuildingInProgress = 2,
        Completed = 3
    }
}
