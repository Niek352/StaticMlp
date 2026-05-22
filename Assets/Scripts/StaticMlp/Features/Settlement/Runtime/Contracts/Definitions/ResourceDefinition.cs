namespace StaticMlp.Features.Settlement
{
    public readonly struct ResourceDefinition
    {
        public readonly ResourceId Id;
        public readonly string DisplayName;
        public readonly ResourceFamily Family;
        public readonly ResourceUsageFlags Usage;
        public readonly bool IsSettlementStored;
        public readonly int StartingSettlementAmount;

        public ResourceDefinition(
            ResourceId id,
            string displayName,
            ResourceFamily family,
            ResourceUsageFlags usage,
            bool isSettlementStored,
            int startingSettlementAmount)
        {
            Id = id;
            DisplayName = displayName;
            Family = family;
            Usage = usage;
            IsSettlementStored = isSettlementStored;
            StartingSettlementAmount = startingSettlementAmount;
        }
    }
}
