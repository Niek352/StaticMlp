using System;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public sealed class ResourcesInventoryConfig : IResource
    {
        public readonly int InventorySlots;
        public readonly float PickupCollectionRadius;
        public readonly float PickupMagnetSpeed;

        public ResourcesInventoryConfig(
            int inventorySlots,
            float pickupCollectionRadius,
            float pickupMagnetSpeed)
        {
            if (inventorySlots != ResourcesInventory.MAX_SLOTS)
                throw new InvalidOperationException($"Resources inventory capacity must be {ResourcesInventory.MAX_SLOTS}.");
            if (pickupCollectionRadius <= 0f)
                throw new InvalidOperationException("Pickup collection radius must be positive.");
            if (pickupMagnetSpeed <= 0f)
                throw new InvalidOperationException("Pickup magnet speed must be positive.");

            InventorySlots = inventorySlots;
            PickupCollectionRadius = pickupCollectionRadius;
            PickupMagnetSpeed = pickupMagnetSpeed;
        }

        public static ResourcesInventoryConfig CreateDefault() =>
            new(
                inventorySlots: ResourcesInventory.MAX_SLOTS,
                pickupCollectionRadius: 1.5f,
                pickupMagnetSpeed: 12f);
    }
}
