using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public static class WorkbenchResourceAccess
    {
        public static int GetInput<TWorld>(World<TWorld>.Entity entity, ResourceId resourceId)
            where TWorld : struct, IWorldType
        {
            ResourceCatalog.Get(resourceId);
            ref readonly var rows = ref entity.Ref<World<TWorld>.Multi<WorkbenchInputResource>>();
            var index = FindInputIndex(in rows, resourceId);
            return index < 0 ? 0 : rows[index].Amount;
        }

        public static int GetOutput<TWorld>(World<TWorld>.Entity entity, ResourceId resourceId)
            where TWorld : struct, IWorldType
        {
            ResourceCatalog.Get(resourceId);
            ref readonly var rows = ref entity.Ref<World<TWorld>.Multi<WorkbenchOutputResource>>();
            var index = FindOutputIndex(in rows, resourceId);
            return index < 0 ? 0 : rows[index].Amount;
        }

        public static void InitializeRows(SW.Entity entity)
        {
            ref var inputs = ref entity.Add<SW.Multi<WorkbenchInputResource>>();
            inputs.Clear();
            ref var outputs = ref entity.Add<SW.Multi<WorkbenchOutputResource>>();
            outputs.Clear();

            for (var i = 0; i < WorkbenchRecipeCatalog.All.Count; i++)
            {
                var recipe = WorkbenchRecipeCatalog.All[i];
                AddInputRows(ref inputs, recipe.Inputs);
                AddOutputRows(ref outputs, recipe.Outputs);
            }
        }

        private static void AddInputRows(ref SW.Multi<WorkbenchInputResource> rows, ResourceAmount[] amounts)
        {
            for (var i = 0; i < amounts.Length; i++)
            {
                var amount = amounts[i];
                ResourceCatalog.Get(amount.Id);
                if (FindInputIndex(in rows, amount.Id) >= 0)
                    continue;

                rows.Add(new WorkbenchInputResource(amount.Id, 0));
            }
        }

        private static void AddOutputRows(ref SW.Multi<WorkbenchOutputResource> rows, ResourceAmount[] amounts)
        {
            for (var i = 0; i < amounts.Length; i++)
            {
                var amount = amounts[i];
                ResourceCatalog.Get(amount.Id);
                if (FindOutputIndex(in rows, amount.Id) >= 0)
                    continue;

                rows.Add(new WorkbenchOutputResource(amount.Id, 0));
            }
        }

        private static int FindInputIndex<TWorld>(in World<TWorld>.Multi<WorkbenchInputResource> rows, ResourceId resourceId)
            where TWorld : struct, IWorldType
        {
            for (var i = 0; i < rows.Length; i++)
            {
                if (rows[i].Id == resourceId)
                    return i;
            }

            return -1;
        }

        private static int FindOutputIndex<TWorld>(in World<TWorld>.Multi<WorkbenchOutputResource> rows, ResourceId resourceId)
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
