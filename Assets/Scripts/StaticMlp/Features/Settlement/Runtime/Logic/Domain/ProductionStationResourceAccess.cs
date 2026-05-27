using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

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

        public static int GetOutput<TWorld>(World<TWorld>.Entity entity, ResourceId resourceId)
            where TWorld : struct, IWorldType
        {
            ResourceCatalog.Get(resourceId);
            ref readonly var rows = ref entity.Ref<World<TWorld>.Multi<ProductionStationOutputResource>>();
            var index = FindOutputIndex(in rows, resourceId);
            return index < 0 ? 0 : rows[index].Amount;
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
                AddOutputRows(ref outputs, recipe.Outputs);
            }

            if (!hasRecipe)
                throw new InvalidOperationException($"Production station id {stationId.Value} has no recipes.");
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
    }
}
