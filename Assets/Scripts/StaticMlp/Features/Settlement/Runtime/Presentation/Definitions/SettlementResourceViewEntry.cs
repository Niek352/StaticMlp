namespace StaticMlp.Features.Settlement
{
    public struct SettlementResourceViewEntry
    {
        public ResourceId Id;
        public int Amount;

        public SettlementResourceViewEntry(ResourceId id, int amount)
        {
            Id = id;
            Amount = amount;
        }
    }
}
