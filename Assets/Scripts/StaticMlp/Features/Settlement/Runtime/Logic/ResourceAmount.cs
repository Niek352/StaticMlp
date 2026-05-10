namespace StaticMlp.Features.Settlement
{
    public readonly struct ResourceAmount
    {
        public readonly ResourceId Id;
        public readonly int Amount;

        public ResourceAmount(ResourceId id, int amount)
        {
            Id = id;
            Amount = amount;
        }
    }
}
