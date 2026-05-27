namespace StaticMlp.Features.Settlement
{
    public enum ProductionStationBlockedReason : byte
    {
        None = 0,
        MissingFuel = 1,
        MissingInputs = 2,
        FullOutputBuffer = 3,
        NoWorkers = 4,
    }
}
