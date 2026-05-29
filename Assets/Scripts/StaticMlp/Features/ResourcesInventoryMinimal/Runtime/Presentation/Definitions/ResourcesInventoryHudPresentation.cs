namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public readonly struct ResourcesInventoryHudPresentation
    {
        public readonly bool IsReady;
        public readonly int UsedSlots;
        public readonly int Capacity;
        public readonly int TotalAmount;
        public readonly ResourcesInventorySlotPresentation[] Slots;

        public ResourcesInventoryHudPresentation(
            bool isReady,
            int usedSlots,
            int capacity,
            int totalAmount,
            ResourcesInventorySlotPresentation[] slots)
        {
            IsReady = isReady;
            UsedSlots = usedSlots;
            Capacity = capacity;
            TotalAmount = totalAmount;
            Slots = slots;
        }

        public static ResourcesInventoryHudPresentation NotReady() =>
            new(false, 0, ResourcesInventory.MAX_SLOTS, 0, System.Array.Empty<ResourcesInventorySlotPresentation>());
    }
}
