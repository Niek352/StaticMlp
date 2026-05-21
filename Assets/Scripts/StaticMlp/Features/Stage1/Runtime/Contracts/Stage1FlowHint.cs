namespace StaticMlp.Features.Settlement
{
    public enum Stage1FlowHint : byte
    {
        None = 0,
        GatherRepairResources = 1,
        ContinueRepairBuild = 2,
        AssignWorker = 3,
        PlaceStockpile = 4,
        PlaceShelter = 5,
        BringExtractionOnline = 6,
        BringWorkbenchOnline = 7,
    }
}
