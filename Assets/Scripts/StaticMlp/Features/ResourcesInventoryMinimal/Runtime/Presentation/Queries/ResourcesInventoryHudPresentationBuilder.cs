using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public static class ResourcesInventoryHudPresentationBuilder
    {
        public static ResourcesInventoryHudPresentation Build()
        {
            var found = false;
            var player = default(CW.Entity);

            foreach (var entity in CW.Query<All<LocalOwned, PlayerTag, ResourcesInventory>>().Entities())
            {
                if (found)
                    throw new InvalidOperationException("Multiple local player resource inventories were found.");

                player = entity;
                found = true;
            }

            if (!found)
                return ResourcesInventoryHudPresentation.NotReady();

            ref readonly var inventory = ref player.Read<ResourcesInventory>();
            ref readonly var rows = ref player.Ref<CW.Multi<CarriedResourceEntry>>();
            ResourcesInventoryAccess.ValidateRows(in rows, inventory.Capacity);

            var slots = new ResourcesInventorySlotPresentation[ResourcesInventory.MAX_SLOTS];
            for (var i = 0; i < slots.Length; i++)
                slots[i] = ResourcesInventorySlotPresentation.Empty();

            for (var i = 0; i < rows.Length; i++)
            {
                var row = rows[i];
                slots[i] = new ResourcesInventorySlotPresentation(
                    true,
                    ResourceCatalog.Get(row.Id).DisplayName,
                    row.Amount);
            }

            return new ResourcesInventoryHudPresentation(true, rows.Length, inventory.Capacity, slots);
        }
    }
}
