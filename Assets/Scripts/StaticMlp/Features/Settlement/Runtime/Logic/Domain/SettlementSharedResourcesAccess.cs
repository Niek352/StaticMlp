using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public static class SettlementSharedResourcesAccess
    {
        public static int GetAmount<TWorld>(World<TWorld>.Entity entity, ResourceId resourceId)
            where TWorld : struct, IWorldType
        {
            ResourceCatalog.Get(resourceId);
            ref readonly var rows = ref entity.Ref<World<TWorld>.Multi<SettlementStoredResource>>();
            var index = FindIndex(in rows, resourceId);
            if (index < 0)
                throw new InvalidOperationException($"Settlement storage is missing resource id {resourceId.Value}.");

            return rows[index].Amount;
        }

        public static int GetProjectedAmount(CW.Entity entity, ResourceId resourceId)
        {
            ResourceCatalog.Get(resourceId);
            ref readonly var rows = ref ClientProjection.ReadMulti<SettlementStoredResource>(entity);
            var index = FindProjectedIndex(in rows, resourceId);
            if (index < 0)
                throw new InvalidOperationException($"Projected settlement storage is missing resource id {resourceId.Value}.");

            return rows[index].Value.Amount;
        }

        public static int TotalUsed<TWorld>(World<TWorld>.Entity entity)
            where TWorld : struct, IWorldType
        {
            ref readonly var rows = ref entity.Ref<World<TWorld>.Multi<SettlementStoredResource>>();
            var total = 0;
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Amount < 0)
                    throw new InvalidOperationException($"Settlement storage resource id {rows[i].Id.Value} has negative amount {rows[i].Amount}.");

                total += rows[i].Amount;
            }

            return total;
        }

        public static int TotalProjectedUsed(CW.Entity entity)
        {
            ref readonly var rows = ref ClientProjection.ReadMulti<SettlementStoredResource>(entity);
            var total = 0;
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Value.Amount < 0)
                    throw new InvalidOperationException($"Projected settlement storage resource id {rows[i].Value.Id.Value} has negative amount {rows[i].Value.Amount}.");

                total += rows[i].Value.Amount;
            }

            return total;
        }

        public static int Add(SW.Entity entity, ResourceId resourceId, int amount)
        {
            ref var host = ref ReplicationMut.Mut<SettlementSharedResources>(entity);
            ref var rows = ref entity.Ref<SW.Multi<SettlementStoredResource>>();
            return Add(ref host, ref rows, resourceId, amount);
        }

        public static int Add(CW.Entity entity, ResourceId resourceId, int amount)
        {
            ref var host = ref ReplicationMut.Mut<SettlementSharedResources>(entity);
            ref var rows = ref entity.Ref<CW.Multi<SettlementStoredResource>>();
            return Add(ref host, ref rows, resourceId, amount);
        }

        public static int Spend(SW.Entity entity, ResourceId resourceId, int requested)
        {
            ReplicationMut.Mut<SettlementSharedResources>(entity);
            ref var rows = ref entity.Ref<SW.Multi<SettlementStoredResource>>();
            return Spend(ref rows, resourceId, requested);
        }

        public static int Spend(CW.Entity entity, ResourceId resourceId, int requested)
        {
            ReplicationMut.Mut<SettlementSharedResources>(entity);
            ref var rows = ref entity.Ref<CW.Multi<SettlementStoredResource>>();
            return Spend(ref rows, resourceId, requested);
        }

        public static int SpendProjected(CW.Entity entity, ResourceId resourceId, int requested)
        {
            ClientProjection.Mut<SettlementSharedResources>(entity);
            ref var rows = ref ClientProjection.MutMulti<SettlementStoredResource>(entity);
            return SpendProjected(ref rows, resourceId, requested);
        }

        public static void InitializeRows(SW.Entity entity, SettlementSeed seed)
        {
            ref var rows = ref entity.Add<SW.Multi<SettlementStoredResource>>();
            rows.Clear();

            var resources = ResourceCatalog.All;
            for (var i = 0; i < resources.Count; i++)
            {
                var definition = resources[i];
                if (!definition.IsSettlementStored)
                    continue;

                rows.Add(new SettlementStoredResource(
                    definition.Id,
                    seed.GetStartingResourceAmount(definition.Id)));
            }
        }

        private static int Add<TWorld>(
            ref SettlementSharedResources host,
            ref World<TWorld>.Multi<SettlementStoredResource> rows,
            ResourceId resourceId,
            int amount)
            where TWorld : struct, IWorldType
        {
            if (amount < 0)
                throw new InvalidOperationException($"Cannot add negative settlement resource amount {amount} for resource id {resourceId.Value}.");

            var totalUsed = TotalUsed(in rows);
            var accepted = StockpileRules.ClampToCapacity(host.Capacity, totalUsed, amount);
            if (accepted <= 0)
                return 0;

            var index = FindRequiredIndex(in rows, resourceId);
            ref var row = ref rows[index];
            row.Amount += accepted;
            return accepted;
        }

        private static int Spend<TWorld>(
            ref World<TWorld>.Multi<SettlementStoredResource> rows,
            ResourceId resourceId,
            int requested)
            where TWorld : struct, IWorldType
        {
            var amount = Math.Max(0, requested);
            var index = FindRequiredIndex(in rows, resourceId);
            ref var row = ref rows[index];
            var spent = Math.Min(amount, row.Amount);
            row.Amount -= spent;
            return spent;
        }

        private static int SpendProjected(
            ref CW.Multi<ProjectedMulti<SettlementStoredResource>> rows,
            ResourceId resourceId,
            int requested)
        {
            var amount = Math.Max(0, requested);
            var index = FindProjectedRequiredIndex(in rows, resourceId);
            ref var row = ref rows[index];
            var spent = Math.Min(amount, row.Value.Amount);
            row.Value.Amount -= spent;
            return spent;
        }

        private static int TotalUsed<TWorld>(in World<TWorld>.Multi<SettlementStoredResource> rows)
            where TWorld : struct, IWorldType
        {
            var total = 0;
            for (var i = 0; i < rows.Length; i++)
                total += rows[i].Amount;

            return total;
        }

        private static int FindRequiredIndex<TWorld>(in World<TWorld>.Multi<SettlementStoredResource> rows, ResourceId resourceId)
            where TWorld : struct, IWorldType
        {
            ResourceCatalog.Get(resourceId);
            var index = FindIndex(in rows, resourceId);
            if (index < 0)
                throw new InvalidOperationException($"Settlement storage is missing resource id {resourceId.Value}.");

            return index;
        }

        private static int FindProjectedRequiredIndex(in CW.Multi<ProjectedMulti<SettlementStoredResource>> rows, ResourceId resourceId)
        {
            ResourceCatalog.Get(resourceId);
            var index = FindProjectedIndex(in rows, resourceId);
            if (index < 0)
                throw new InvalidOperationException($"Projected settlement storage is missing resource id {resourceId.Value}.");

            return index;
        }

        private static int FindIndex<TWorld>(in World<TWorld>.Multi<SettlementStoredResource> rows, ResourceId resourceId)
            where TWorld : struct, IWorldType
        {
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Id == resourceId)
                    return i;
            }

            return -1;
        }

        private static int FindProjectedIndex(in CW.Multi<ProjectedMulti<SettlementStoredResource>> rows, ResourceId resourceId)
        {
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Value.Id == resourceId)
                    return i;
            }

            return -1;
        }
    }
}
