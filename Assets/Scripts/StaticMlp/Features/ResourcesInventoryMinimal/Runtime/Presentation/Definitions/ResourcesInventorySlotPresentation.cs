namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public readonly struct ResourcesInventorySlotPresentation
    {
        public readonly bool IsOccupied;
        public readonly string ResourceName;
        public readonly int Amount;

        public ResourcesInventorySlotPresentation(
            bool isOccupied,
            string resourceName,
            int amount)
        {
            IsOccupied = isOccupied;
            ResourceName = resourceName;
            Amount = amount;
        }

        public static ResourcesInventorySlotPresentation Empty() =>
            new(false, string.Empty, 0);
    }
}
