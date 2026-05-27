using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public static class ConstructionResourcesAccess
    {
        public static int GetRequired<TWorld>(World<TWorld>.Entity entity, ResourceId resourceId)
            where TWorld : struct, IWorldType
        {
            ResourceCatalog.Get(resourceId);
            ref readonly var rows = ref entity.Ref<World<TWorld>.Multi<ConstructionResourceEntry>>();
            var index = FindIndex(in rows, resourceId);
            return index < 0 ? 0 : rows[index].Required;
        }

        public static int GetProjectedRequired(CW.Entity entity, ResourceId resourceId)
        {
            ResourceCatalog.Get(resourceId);
            ref readonly var rows = ref ClientProjection.ReadMulti<ConstructionResourceEntry>(entity);
            var index = FindProjectedIndex(in rows, resourceId);
            return index < 0 ? 0 : rows[index].Value.Required;
        }

        public static int GetDelivered<TWorld>(World<TWorld>.Entity entity, ResourceId resourceId)
            where TWorld : struct, IWorldType
        {
            ResourceCatalog.Get(resourceId);
            ref readonly var rows = ref entity.Ref<World<TWorld>.Multi<ConstructionResourceEntry>>();
            var index = FindIndex(in rows, resourceId);
            return index < 0 ? 0 : rows[index].Delivered;
        }

        public static int GetProjectedDelivered(CW.Entity entity, ResourceId resourceId)
        {
            ResourceCatalog.Get(resourceId);
            ref readonly var rows = ref ClientProjection.ReadMulti<ConstructionResourceEntry>(entity);
            var index = FindProjectedIndex(in rows, resourceId);
            return index < 0 ? 0 : rows[index].Value.Delivered;
        }

        public static int GetRemaining<TWorld>(World<TWorld>.Entity entity, ResourceId resourceId)
            where TWorld : struct, IWorldType
        {
            return Math.Max(0, GetRequired(entity, resourceId) - GetDelivered(entity, resourceId));
        }

        public static int GetProjectedRemaining(CW.Entity entity, ResourceId resourceId)
        {
            return Math.Max(0, GetProjectedRequired(entity, resourceId) - GetProjectedDelivered(entity, resourceId));
        }

        public static bool IsComplete<TWorld>(World<TWorld>.Entity entity)
            where TWorld : struct, IWorldType
        {
            ref readonly var rows = ref entity.Ref<World<TWorld>.Multi<ConstructionResourceEntry>>();
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Delivered < rows[i].Required)
                    return false;
            }

            return true;
        }

        public static bool IsProjectedComplete(CW.Entity entity)
        {
            ref readonly var rows = ref ClientProjection.ReadMulti<ConstructionResourceEntry>(entity);
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Value.Delivered < rows[i].Value.Required)
                    return false;
            }

            return true;
        }

        public static int Deliver(SW.Entity entity, ResourceId resourceId, int requested)
        {
            ReplicationMut.Mut<ConstructionResources>(entity);
            ref var rows = ref entity.Ref<SW.Multi<ConstructionResourceEntry>>();
            return Deliver(ref rows, resourceId, requested);
        }

        public static int Deliver(CW.Entity entity, ResourceId resourceId, int requested)
        {
            ReplicationMut.Mut<ConstructionResources>(entity);
            ref var rows = ref entity.Ref<CW.Multi<ConstructionResourceEntry>>();
            return Deliver(ref rows, resourceId, requested);
        }

        public static int DeliverProjected(CW.Entity entity, ResourceId resourceId, int requested)
        {
            ClientProjection.Mut<ConstructionResources>(entity);
            ref var rows = ref ClientProjection.MutMulti<ConstructionResourceEntry>(entity);
            return DeliverProjected(ref rows, resourceId, requested);
        }

        public static void MarkAllDelivered(SW.Entity entity)
        {
            ReplicationMut.Mut<ConstructionResources>(entity);
            ref var rows = ref entity.Ref<SW.Multi<ConstructionResourceEntry>>();
            for (var i = 0; i < rows.Length; i++)
            {
                ref var row = ref rows[i];
                row.Delivered = row.Required;
            }
        }

        public static void InitializeRows(SW.Entity entity, ResourceAmount[] constructionCost)
        {
            ref var rows = ref entity.Add<SW.Multi<ConstructionResourceEntry>>();
            rows.Clear();

            for (var i = 0; i < constructionCost.Length; i++)
            {
                var cost = constructionCost[i];
                ValidateResourceAmount(cost.Id, cost.Amount);
                if (FindIndex(in rows, cost.Id) >= 0)
                    throw new InvalidOperationException($"Duplicate construction resource id {cost.Id.Value}.");

                rows.Add(new ConstructionResourceEntry(cost.Id, cost.Amount));
            }
        }

        public static ResourceAmount[] GetRemainingResources<TWorld>(World<TWorld>.Entity entity)
            where TWorld : struct, IWorldType
        {
            ref readonly var rows = ref entity.Ref<World<TWorld>.Multi<ConstructionResourceEntry>>();
            var resources = new ResourceAmount[rows.Length];
            for (var i = 0; i < rows.Length; i++)
                resources[i] = new ResourceAmount(rows.Get(i).Id, Math.Max(0, rows[i].Required - rows[i].Delivered));

            return resources;
        }

        public static ResourceAmount[] GetProjectedRemainingResources(CW.Entity entity)
        {
            ref readonly var rows = ref ClientProjection.ReadMulti<ConstructionResourceEntry>(entity);
            var resources = new ResourceAmount[rows.Length];
            for (var i = 0; i < rows.Length; i++)
                resources[i] = new ResourceAmount(rows.Get(i).Value.Id, Math.Max(0, rows[i].Value.Required - rows[i].Value.Delivered));

            return resources;
        }

        private static int Deliver<TWorld>(
            ref World<TWorld>.Multi<ConstructionResourceEntry> rows,
            ResourceId resourceId,
            int requested)
            where TWorld : struct, IWorldType
        {
            var amount = Math.Max(0, requested);
            var index = FindRequiredIndex(in rows, resourceId);
            ref var row = ref rows[index];
            var delivered = Math.Min(amount, Math.Max(0, row.Required - row.Delivered));
            row.Delivered += delivered;
            return delivered;
        }

        private static int DeliverProjected(
            ref CW.Multi<ProjectedMulti<ConstructionResourceEntry>> rows,
            ResourceId resourceId,
            int requested)
        {
            var amount = Math.Max(0, requested);
            var index = FindProjectedRequiredIndex(in rows, resourceId);
            ref var row = ref rows[index];
            var delivered = Math.Min(amount, Math.Max(0, row.Value.Required - row.Value.Delivered));
            row.Value.Delivered += delivered;
            return delivered;
        }

        private static int FindRequiredIndex<TWorld>(in World<TWorld>.Multi<ConstructionResourceEntry> rows, ResourceId resourceId)
            where TWorld : struct, IWorldType
        {
            ResourceCatalog.Get(resourceId);
            var index = FindIndex(in rows, resourceId);
            if (index < 0)
                throw new InvalidOperationException($"Construction resources are missing resource id {resourceId.Value}.");

            return index;
        }

        private static int FindProjectedRequiredIndex(in CW.Multi<ProjectedMulti<ConstructionResourceEntry>> rows, ResourceId resourceId)
        {
            ResourceCatalog.Get(resourceId);
            var index = FindProjectedIndex(in rows, resourceId);
            if (index < 0)
                throw new InvalidOperationException($"Projected construction resources are missing resource id {resourceId.Value}.");

            return index;
        }

        private static int FindIndex<TWorld>(in World<TWorld>.Multi<ConstructionResourceEntry> rows, ResourceId resourceId)
            where TWorld : struct, IWorldType
        {
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Id == resourceId)
                    return i;
            }

            return -1;
        }

        private static int FindProjectedIndex(in CW.Multi<ProjectedMulti<ConstructionResourceEntry>> rows, ResourceId resourceId)
        {
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Value.Id == resourceId)
                    return i;
            }

            return -1;
        }

        private static void ValidateResourceAmount(ResourceId id, int amount)
        {
            ResourceCatalog.Get(id);
            if (amount < 0)
                throw new InvalidOperationException($"Construction resource id {id.Value} has negative required amount {amount}.");
        }
    }
}
