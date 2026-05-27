using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public static class ProductionStationResourceAccess
    {
        public static int GetInput<TWorld>(World<TWorld>.Entity entity, ResourceId resourceId)
            where TWorld : struct, IWorldType
        {
            ResourceCatalog.Get(resourceId);
            ref readonly var rows = ref entity.Ref<World<TWorld>.Multi<ProductionStationInputResource>>();
            var index = FindInputIndex(in rows, resourceId);
            return index < 0 ? 0 : rows[index].Amount;
        }

        public static int GetProjectedInput(CW.Entity entity, ResourceId resourceId)
        {
            ResourceCatalog.Get(resourceId);
            ref readonly var rows = ref ClientProjection.ReadMulti<ProductionStationInputResource>(entity);
            var index = FindProjectedInputIndex(in rows, resourceId);
            return index < 0 ? 0 : rows[index].Value.Amount;
        }

        public static int GetOutput<TWorld>(World<TWorld>.Entity entity, ResourceId resourceId)
            where TWorld : struct, IWorldType
        {
            ResourceCatalog.Get(resourceId);
            ref readonly var rows = ref entity.Ref<World<TWorld>.Multi<ProductionStationOutputResource>>();
            var index = FindOutputIndex(in rows, resourceId);
            return index < 0 ? 0 : rows[index].Amount;
        }

        public static int GetProjectedOutput(CW.Entity entity, ResourceId resourceId)
        {
            ResourceCatalog.Get(resourceId);
            ref readonly var rows = ref ClientProjection.ReadMulti<ProductionStationOutputResource>(entity);
            var index = FindProjectedOutputIndex(in rows, resourceId);
            return index < 0 ? 0 : rows[index].Value.Amount;
        }

        public static int TotalOutputAmount<TWorld>(World<TWorld>.Entity entity)
            where TWorld : struct, IWorldType
        {
            ref readonly var rows = ref entity.Ref<World<TWorld>.Multi<ProductionStationOutputResource>>();
            var total = 0;
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Amount < 0)
                    throw new InvalidOperationException(
                        $"Production station output resource id {rows[i].Id.Value} has negative amount {rows[i].Amount}.");

                total += rows[i].Amount;
            }

            return total;
        }

        public static int TotalProjectedOutputAmount(CW.Entity entity)
        {
            ref readonly var rows = ref ClientProjection.ReadMulti<ProductionStationOutputResource>(entity);
            var total = 0;
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Value.Amount < 0)
                    throw new InvalidOperationException(
                        $"Projected production station output resource id {rows[i].Value.Id.Value} has negative amount {rows[i].Value.Amount}.");

                total += rows[i].Value.Amount;
            }

            return total;
        }

        public static int AddInput(SW.Entity entity, ResourceId resourceId, int requestedAmount)
        {
            ReplicationMut.Mut<ProductionStationOperationState>(entity);
            ref var rows = ref entity.Ref<SW.Multi<ProductionStationInputResource>>();
            return AddInput(ref rows, resourceId, requestedAmount);
        }

        public static int SpendInput(SW.Entity entity, ResourceId resourceId, int requestedAmount)
        {
            ReplicationMut.Mut<ProductionStationOperationState>(entity);
            ref var rows = ref entity.Ref<SW.Multi<ProductionStationInputResource>>();
            return SpendInput(ref rows, resourceId, requestedAmount);
        }

        public static int AddOutput(SW.Entity entity, ResourceId resourceId, int requestedAmount)
        {
            ReplicationMut.Mut<ProductionStationOperationState>(entity);
            ref var rows = ref entity.Ref<SW.Multi<ProductionStationOutputResource>>();
            return AddOutput(ref rows, resourceId, requestedAmount);
        }

        public static int RemoveOutput(SW.Entity entity, ResourceId resourceId, int requestedAmount)
        {
            ReplicationMut.Mut<ProductionStationOperationState>(entity);
            ref var rows = ref entity.Ref<SW.Multi<ProductionStationOutputResource>>();
            return RemoveOutput(ref rows, resourceId, requestedAmount);
        }

        public static void InitializeRows(SW.Entity entity, ProductionStationId stationId)
        {
            ref var inputs = ref entity.Add<SW.Multi<ProductionStationInputResource>>();
            inputs.Clear();
            ref var outputs = ref entity.Add<SW.Multi<ProductionStationOutputResource>>();
            outputs.Clear();

            var hasRecipe = false;
            for (var i = 0; i < ProductionRecipeCatalog.All.Count; i++)
            {
                var recipe = ProductionRecipeCatalog.All[i];
                if (recipe.StationId != stationId)
                    continue;

                hasRecipe = true;
                AddInputRows(ref inputs, recipe.Inputs);
                if (recipe.FuelRequirement.HasValue)
                    AddInputRows(ref inputs, new[] { recipe.FuelRequirement.Value });
                AddOutputRows(ref outputs, recipe.Outputs);
            }

            if (!hasRecipe)
                throw new InvalidOperationException($"Production station id {stationId.Value} has no recipes.");
        }

        private static int AddInput(
            ref SW.Multi<ProductionStationInputResource> rows,
            ResourceId resourceId,
            int requestedAmount)
        {
            if (requestedAmount < 0)
                throw new InvalidOperationException($"Cannot add negative production input amount {requestedAmount}.");

            if (requestedAmount == 0)
                return 0;

            var index = FindRequiredInputIndex(in rows, resourceId);
            ref var row = ref rows[index];
            row.Amount += requestedAmount;
            return requestedAmount;
        }

        private static int SpendInput(
            ref SW.Multi<ProductionStationInputResource> rows,
            ResourceId resourceId,
            int requestedAmount)
        {
            if (requestedAmount < 0)
                throw new InvalidOperationException($"Cannot spend negative production input amount {requestedAmount}.");

            if (requestedAmount == 0)
                return 0;

            var index = FindRequiredInputIndex(in rows, resourceId);
            ref var row = ref rows[index];
            var spent = Math.Min(requestedAmount, row.Amount);
            row.Amount -= spent;
            return spent;
        }

        private static int AddOutput(
            ref SW.Multi<ProductionStationOutputResource> rows,
            ResourceId resourceId,
            int requestedAmount)
        {
            if (requestedAmount < 0)
                throw new InvalidOperationException($"Cannot add negative production output amount {requestedAmount}.");

            if (requestedAmount == 0)
                return 0;

            var index = FindRequiredOutputIndex(in rows, resourceId);
            ref var row = ref rows[index];
            row.Amount += requestedAmount;
            return requestedAmount;
        }

        private static int RemoveOutput(
            ref SW.Multi<ProductionStationOutputResource> rows,
            ResourceId resourceId,
            int requestedAmount)
        {
            if (requestedAmount < 0)
                throw new InvalidOperationException($"Cannot remove negative production output amount {requestedAmount}.");

            if (requestedAmount == 0)
                return 0;

            var index = FindRequiredOutputIndex(in rows, resourceId);
            ref var row = ref rows[index];
            var removed = Math.Min(requestedAmount, row.Amount);
            row.Amount -= removed;
            return removed;
        }

        private static int FindRequiredInputIndex<TWorld>(
            in World<TWorld>.Multi<ProductionStationInputResource> rows,
            ResourceId resourceId)
            where TWorld : struct, IWorldType
        {
            ResourceCatalog.Get(resourceId);
            var index = FindInputIndex(in rows, resourceId);
            if (index < 0)
                throw new InvalidOperationException($"Production station inputs are missing resource id {resourceId.Value}.");

            return index;
        }

        private static int FindRequiredOutputIndex<TWorld>(
            in World<TWorld>.Multi<ProductionStationOutputResource> rows,
            ResourceId resourceId)
            where TWorld : struct, IWorldType
        {
            ResourceCatalog.Get(resourceId);
            var index = FindOutputIndex(in rows, resourceId);
            if (index < 0)
                throw new InvalidOperationException($"Production station outputs are missing resource id {resourceId.Value}.");

            return index;
        }

        private static void AddInputRows(ref SW.Multi<ProductionStationInputResource> rows, ResourceAmount[] amounts)
        {
            for (var i = 0; i < amounts.Length; i++)
            {
                var amount = amounts[i];
                ResourceCatalog.Get(amount.Id);
                if (FindInputIndex(in rows, amount.Id) >= 0)
                    continue;

                rows.Add(new ProductionStationInputResource(amount.Id, 0));
            }
        }

        private static void AddOutputRows(ref SW.Multi<ProductionStationOutputResource> rows, ResourceAmount[] amounts)
        {
            for (var i = 0; i < amounts.Length; i++)
            {
                var amount = amounts[i];
                ResourceCatalog.Get(amount.Id);
                if (FindOutputIndex(in rows, amount.Id) >= 0)
                    continue;

                rows.Add(new ProductionStationOutputResource(amount.Id, 0));
            }
        }

        private static int FindInputIndex<TWorld>(
            in World<TWorld>.Multi<ProductionStationInputResource> rows,
            ResourceId resourceId)
            where TWorld : struct, IWorldType
        {
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Id == resourceId)
                    return i;
            }

            return -1;
        }

        private static int FindOutputIndex<TWorld>(
            in World<TWorld>.Multi<ProductionStationOutputResource> rows,
            ResourceId resourceId)
            where TWorld : struct, IWorldType
        {
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Id == resourceId)
                    return i;
            }

            return -1;
        }

        private static int FindProjectedInputIndex(
            in CW.Multi<ProjectedMulti<ProductionStationInputResource>> rows,
            ResourceId resourceId)
        {
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Value.Id == resourceId)
                    return i;
            }

            return -1;
        }

        private static int FindProjectedOutputIndex(
            in CW.Multi<ProjectedMulti<ProductionStationOutputResource>> rows,
            ResourceId resourceId)
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
