namespace StaticMlp.Features.Settlement.Workers
{
    public enum SettlementWorkerBlockingReason : byte
    {
        None = 0,
        NoAssignment = 1,
        AwaitingCampRepair = 2,
        NoConstructionDemand = 3,
        MissingResources = 4,
    }
}
