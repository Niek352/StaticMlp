namespace StaticMlp.Features.Settlement
{
    public readonly struct ResourceDefinition
    {
        public readonly ResourceId Id;
        public readonly ResourceUsageFlags Usage;
        public readonly bool IsSettlementStored;
        public readonly int StartingSettlementAmount;

        public ResourceDefinition(
            ResourceId id,
            ResourceUsageFlags usage,
            bool isSettlementStored,
            int startingSettlementAmount)
        {
            Id = id;
            Usage = usage;
            IsSettlementStored = isSettlementStored;
            StartingSettlementAmount = startingSettlementAmount;
        }
    }
}
