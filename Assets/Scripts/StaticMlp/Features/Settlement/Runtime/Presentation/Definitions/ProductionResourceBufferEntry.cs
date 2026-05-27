namespace StaticMlp.Features.Settlement
{
    public readonly struct ProductionResourceBufferEntry
    {
        public readonly ResourceId Id;
        public readonly int Amount;
        public readonly int BatchAmount;

        public ProductionResourceBufferEntry(ResourceId id, int amount, int batchAmount)
        {
            Id = id;
            Amount = amount;
            BatchAmount = batchAmount;
        }
    }
}
