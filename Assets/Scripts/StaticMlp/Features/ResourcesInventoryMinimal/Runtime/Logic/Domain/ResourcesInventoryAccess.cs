using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public static class ResourcesInventoryAccess
    {
        public static void Initialize(SW.Entity entity, int capacity)
        {
            ResourcesInventory.ValidateCapacity(capacity);
            entity.Set(new ResourcesInventory
            {
                Capacity = capacity
            });
            entity.Add<SW.Multi<CarriedResourceEntry>>();
            ReplicationMut.MarkDataDirty(entity);
        }

        public static int TotalAmount<TWorld>(World<TWorld>.Entity entity)
            where TWorld : struct, IWorldType
        {
            ref readonly var inventory = ref entity.Read<ResourcesInventory>();
            ref readonly var rows = ref entity.Ref<World<TWorld>.Multi<CarriedResourceEntry>>();
            ValidateRows(in rows, inventory.Capacity);
            return TotalUsed(in rows);
        }

        public static int GetAmount<TWorld>(World<TWorld>.Entity entity, ResourceId id)
            where TWorld : struct, IWorldType
        {
            ValidateRawResourceId(id);
            ref readonly var inventory = ref entity.Read<ResourcesInventory>();
            ref readonly var rows = ref entity.Ref<World<TWorld>.Multi<CarriedResourceEntry>>();
            ValidateRows(in rows, inventory.Capacity);

            var index = FindIndex(in rows, id);
            return index >= 0 ? rows[index].Amount : 0;
        }

        public static int Add(SW.Entity entity, ResourceAmount resource)
        {
            ValidateResourceAmount(resource);

            ref var inventory = ref ReplicationMut.Mut<ResourcesInventory>(entity);
            ref var rows = ref entity.Ref<SW.Multi<CarriedResourceEntry>>();
            ValidateRows(in rows, inventory.Capacity);

            if (resource.Amount == 0)
                return 0;

            var available = inventory.Capacity - TotalUsed(in rows);
            var accepted = Math.Min(resource.Amount, available);
            if (accepted <= 0)
                return resource.Amount;

            var index = FindIndex(in rows, resource.Id);
            if (index >= 0)
            {
                ref var row = ref rows[index];
                row.Amount += accepted;
            }
            else
            {
                rows.Add(new CarriedResourceEntry(resource.Id, accepted));
            }

            ValidateRows(in rows, inventory.Capacity);
            return resource.Amount - accepted;
        }

        public static int Spend(SW.Entity entity, ResourceId id, int requested)
        {
            ValidateRawResourceId(id);
            if (requested < 0)
                throw new InvalidOperationException($"Cannot spend negative resource amount {requested}.");
            if (requested == 0)
                return 0;

            ref var inventory = ref ReplicationMut.Mut<ResourcesInventory>(entity);
            ref var rows = ref entity.Ref<SW.Multi<CarriedResourceEntry>>();
            ValidateRows(in rows, inventory.Capacity);

            var index = FindIndex(in rows, id);
            if (index < 0)
                return 0;

            ref var row = ref rows[index];
            var spent = Math.Min(requested, row.Amount);
            row.Amount -= spent;
            if (row.Amount > 0)
            {
                ValidateRows(in rows, inventory.Capacity);
                return spent;
            }
            else
            {
                rows.RemoveAt(index);
            }

            ValidateRows(in rows, inventory.Capacity);
            return spent;
        }

        public static void ValidateRows<TWorld>(in World<TWorld>.Multi<CarriedResourceEntry> rows, int capacity)
            where TWorld : struct, IWorldType
        {
            ResourcesInventory.ValidateCapacity(capacity);
            if (rows.Length > ResourceCatalog.All.Count || rows.Length > capacity)
                throw new InvalidOperationException($"Invalid carried resource row count {rows.Length}.");

            var total = 0;
            for (var i = 0; i < rows.Length; i++)
            {
                ValidateRow(rows[i].Id, rows[i].Amount);
                total += rows[i].Amount;
                if (total > capacity)
                    throw new InvalidOperationException($"Carried resource total {total} exceeds capacity {capacity}.");

                for (var j = i + 1; j < rows.Length; j++)
                {
                    if (rows[i].Id == rows[j].Id)
                        throw new InvalidOperationException($"Duplicate carried resource id {rows[i].Id.Value}.");
                }
            }
        }

        public static void ValidateRow(ResourceId id, int amount)
        {
            ValidateRawResourceId(id);
            if (amount <= 0)
                throw new InvalidOperationException($"Carried resource id {id.Value} has invalid amount {amount}.");
        }

        public static int FindIndex<TWorld>(in World<TWorld>.Multi<CarriedResourceEntry> rows, ResourceId id)
            where TWorld : struct, IWorldType
        {
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Id == id)
                    return i;
            }

            return -1;
        }

        private static void ValidateResourceAmount(ResourceAmount resource)
        {
            ValidateRawResourceId(resource.Id);
            if (resource.Amount < 0)
                throw new InvalidOperationException($"Invalid negative resource amount {resource.Amount} for resource id {resource.Id.Value}.");
        }

        private static void ValidateRawResourceId(ResourceId id)
        {
            if (id.Value == 0)
                throw new InvalidOperationException("Resource id 0 cannot be stored in carried inventory.");

            ref readonly var definition = ref ResourceCatalog.Get(id);
            if (definition.Family != ResourceFamily.Raw)
                throw new InvalidOperationException($"Resource id {id.Value} is {definition.Family}, not raw.");
        }

        private static int TotalUsed<TWorld>(in World<TWorld>.Multi<CarriedResourceEntry> rows)
            where TWorld : struct, IWorldType
        {
            var total = 0;
            for (var i = 0; i < rows.Length; i++)
                total += rows[i].Amount;

            return total;
        }
    }
}
