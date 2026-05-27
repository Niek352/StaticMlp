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

            var amount = 0;
            for (var i = 0; i < rows.Length; i++)
            {
                var row = rows.Get(i);
                if (row.Id == id)
                    amount += row.Amount;
            }

            return amount;
        }

        public static int CopyAmounts<TWorld>(World<TWorld>.Entity entity, ResourceAmount[] target)
            where TWorld : struct, IWorldType
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            ref readonly var inventory = ref entity.Read<ResourcesInventory>();
            ref readonly var rows = ref entity.Ref<World<TWorld>.Multi<CarriedResourceEntry>>();
            ValidateRows(in rows, inventory.Capacity);

            if (target.Length < rows.Length)
                throw new InvalidOperationException(
                    $"Target resource amount buffer has {target.Length} slots, but carried inventory has {rows.Length} rows.");

            for (var i = 0; i < rows.Length; i++)
            {
                var row = rows.Get(i);
                target[i] = new ResourceAmount(row.Id, row.Amount);
            }

            return rows.Length;
        }

        public static int Add(SW.Entity entity, ResourceAmount resource)
        {
            ValidateResourceAmount(resource);

            ref var inventory = ref ReplicationMut.Mut<ResourcesInventory>(entity);
            ref var rows = ref entity.Ref<SW.Multi<CarriedResourceEntry>>();
            ValidateRows(in rows, inventory.Capacity);

            if (resource.Amount == 0)
                return 0;

            var remaining = resource.Amount;
            for (var i = 0; i < rows.Length && remaining > 0; i++)
            {
                ref var row = ref rows[i];
                if (row.Id != resource.Id || row.Amount >= ResourcesInventory.MAX_STACK_AMOUNT)
                    continue;

                var accepted = Math.Min(remaining, ResourcesInventory.MAX_STACK_AMOUNT - row.Amount);
                row.Amount += accepted;
                remaining -= accepted;
            }

            while (remaining > 0 && rows.Length < inventory.Capacity)
            {
                var accepted = Math.Min(remaining, ResourcesInventory.MAX_STACK_AMOUNT);
                rows.Add(new CarriedResourceEntry(resource.Id, accepted));
                remaining -= accepted;
            }

            ValidateRows(in rows, inventory.Capacity);
            return remaining;
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

            var remaining = requested;
            var spent = 0;
            for (var i = 0; i < rows.Length && remaining > 0;)
            {
                ref var row = ref rows[i];
                if (row.Id != id)
                {
                    i++;
                    continue;
                }

                var accepted = Math.Min(remaining, row.Amount);
                row.Amount -= accepted;
                remaining -= accepted;
                spent += accepted;

                if (row.Amount == 0)
                    rows.RemoveAt(i);
                else
                    i++;
            }

            ValidateRows(in rows, inventory.Capacity);
            return spent;
        }

        public static void ValidateRows<TWorld>(in World<TWorld>.Multi<CarriedResourceEntry> rows, int capacity)
            where TWorld : struct, IWorldType
        {
            ResourcesInventory.ValidateCapacity(capacity);
            if (rows.Length > capacity)
                throw new InvalidOperationException($"Invalid carried resource row count {rows.Length}.");

            for (var i = 0; i < rows.Length; i++)
            {
                var row = rows.Get(i);
                ValidateRow(row.Id, row.Amount);
            }
        }

        public static void ValidateRow(ResourceId id, int amount)
        {
            ValidateRawResourceId(id);
            if (amount <= 0)
                throw new InvalidOperationException($"Carried resource id {id.Value} has invalid amount {amount}.");
            if (amount > ResourcesInventory.MAX_STACK_AMOUNT)
                throw new InvalidOperationException(
                    $"Carried resource id {id.Value} stack amount {amount} exceeds {ResourcesInventory.MAX_STACK_AMOUNT}.");
        }

        public static int FindIndex<TWorld>(in World<TWorld>.Multi<CarriedResourceEntry> rows, ResourceId id)
            where TWorld : struct, IWorldType
        {
            for (var i = 0; i < rows.Length; i++)
            {
                var row = rows.Get(i);
                if (row.Id == id)
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
            {
                var row = rows.Get(i);
                total += row.Amount;
            }

            return total;
        }
    }
}
