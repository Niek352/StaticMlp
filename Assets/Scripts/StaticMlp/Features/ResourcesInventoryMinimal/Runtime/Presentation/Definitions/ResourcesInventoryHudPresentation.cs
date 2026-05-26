namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public readonly struct ResourcesInventoryHudPresentation
    {
        public readonly bool IsReady;
        public readonly int UsedSlots;
        public readonly int Capacity;
        public readonly ResourcesInventorySlotPresentation[] Slots;

        public ResourcesInventoryHudPresentation(
            bool isReady,
            int usedSlots,
            int capacity,
            ResourcesInventorySlotPresentation[] slots)
        {
            IsReady = isReady;
            UsedSlots = usedSlots;
            Capacity = capacity;
            Slots = slots;
        }

        public static ResourcesInventoryHudPresentation NotReady() =>
            new(false, 0, ResourcesInventory.MAX_SLOTS, System.Array.Empty<ResourcesInventorySlotPresentation>());
    }
}
