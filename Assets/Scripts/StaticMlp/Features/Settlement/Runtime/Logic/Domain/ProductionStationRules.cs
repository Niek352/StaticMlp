using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public static class ProductionStationRules
    {
        public static int ResolveWorkerMultiplier(byte workerSlotCount, int assignedWorkerCount)
        {
            if (workerSlotCount == 0)
                return 1;

            if (assignedWorkerCount <= 0)
                return 0;

            return Math.Min(assignedWorkerCount, workerSlotCount);
        }

        public static bool CanFitOutputs<TWorld>(
            World<TWorld>.Entity station,
            in ProductionRecipeDefinition recipe,
            int outputCapacity)
            where TWorld : struct, IWorldType
        {
            if (outputCapacity <= 0)
                return false;

            var currentAmount = ProductionStationResourceAccess.TotalOutputAmount(station);
            return currentAmount + TotalAmount(recipe.Outputs) <= outputCapacity;
        }

        public static bool HasRecipeInputs<TWorld>(
            World<TWorld>.Entity station,
            in ProductionRecipeDefinition recipe)
            where TWorld : struct, IWorldType
        {
            for (var i = 0; i < recipe.Inputs.Length; i++)
            {
                if (!HasInput(station, recipe.Inputs[i]))
                    return false;
            }

            return !recipe.FuelRequirement.HasValue
                   || HasInput(station, recipe.FuelRequirement.Value);
        }

        public static bool IsBlockedByFuel(
            SW.Entity station,
            SW.Entity sharedStorage,
            in ProductionRecipeDefinition recipe)
        {
            if (!recipe.FuelRequirement.HasValue)
                return false;

            var fuel = recipe.FuelRequirement.Value;
            var stationHas = ProductionStationResourceAccess.GetInput(station, fuel.Id);
            if (stationHas >= fuel.Amount)
                return false;

            var missing = fuel.Amount - stationHas;
            return SettlementSharedResourcesAccess.GetAmount(sharedStorage, fuel.Id) < missing;
        }

        public static bool TryReserveRecipeInputs(
            SW.Entity station,
            SW.Entity sharedStorage,
            in ProductionRecipeDefinition recipe)
        {
            if (HasRecipeInputs(station, in recipe))
                return true;

            if (!HasSharedStorageForMissingInputs(station, sharedStorage, in recipe))
                return false;

            ReserveInputs(station, sharedStorage, recipe.Inputs);
            if (recipe.FuelRequirement.HasValue)
                ReserveInput(station, sharedStorage, recipe.FuelRequirement.Value);

            return true;
        }

        public static bool AdvanceWork(
            ref ProductionStationOperationState state,
            in ProductionRecipeDefinition recipe,
            float workAmount)
        {
            if (state.WorkDone < 0f)
                throw new InvalidOperationException($"Production station has negative work progress {state.WorkDone}.");

            if (workAmount < 0f)
                throw new InvalidOperationException($"Cannot advance production by negative work amount {workAmount}.");

            if (workAmount == 0f)
                return false;

            state.WorkDone += workAmount;
            if (state.WorkDone < recipe.WorkRequired)
                return false;

            state.WorkDone = 0f;
            return true;
        }

        public static void CommitCompletedRecipe(SW.Entity station, in ProductionRecipeDefinition recipe)
        {
            SpendInputs(station, recipe.Inputs);
            if (recipe.FuelRequirement.HasValue)
                SpendInput(station, recipe.FuelRequirement.Value);

            for (var i = 0; i < recipe.Outputs.Length; i++)
            {
                var output = recipe.Outputs[i];
                var added = ProductionStationResourceAccess.AddOutput(station, output.Id, output.Amount);
                if (added != output.Amount)
                    throw new InvalidOperationException(
                        $"Production output added {added} of required {output.Amount} for resource id {output.Id.Value}.");
            }
        }

        public static bool IsStationOutputResource(ProductionStationId stationId, ResourceId resourceId)
        {
            for (var i = 0; i < ProductionRecipeCatalog.All.Count; i++)
            {
                var recipe = ProductionRecipeCatalog.All[i];
                if (recipe.StationId != stationId)
                    continue;

                for (var outputIndex = 0; outputIndex < recipe.Outputs.Length; outputIndex++)
                {
                    if (recipe.Outputs[outputIndex].Id == resourceId)
                        return true;
                }
            }

            return false;
        }

        private static bool HasInput<TWorld>(World<TWorld>.Entity station, ResourceAmount amount)
            where TWorld : struct, IWorldType
        {
            return ProductionStationResourceAccess.GetInput(station, amount.Id) >= amount.Amount;
        }

        private static bool HasSharedStorageForMissingInputs(
            SW.Entity station,
            SW.Entity sharedStorage,
            in ProductionRecipeDefinition recipe)
        {
            for (var i = 0; i < recipe.Inputs.Length; i++)
            {
                if (!HasSharedStorageForMissingInput(station, sharedStorage, recipe.Inputs[i]))
                    return false;
            }

            return !recipe.FuelRequirement.HasValue
                   || HasSharedStorageForMissingInput(station, sharedStorage, recipe.FuelRequirement.Value);
        }

        private static bool HasSharedStorageForMissingInput(
            SW.Entity station,
            SW.Entity sharedStorage,
            ResourceAmount amount)
        {
            var missing = Math.Max(0, amount.Amount - ProductionStationResourceAccess.GetInput(station, amount.Id));
            return missing == 0 || SettlementSharedResourcesAccess.GetAmount(sharedStorage, amount.Id) >= missing;
        }

        private static void ReserveInputs(
            SW.Entity station,
            SW.Entity sharedStorage,
            ResourceAmount[] amounts)
        {
            for (var i = 0; i < amounts.Length; i++)
                ReserveInput(station, sharedStorage, amounts[i]);
        }

        private static void ReserveInput(
            SW.Entity station,
            SW.Entity sharedStorage,
            ResourceAmount amount)
        {
            var missing = Math.Max(0, amount.Amount - ProductionStationResourceAccess.GetInput(station, amount.Id));
            if (missing == 0)
                return;

            var spent = SettlementSharedResourcesAccess.Spend(sharedStorage, amount.Id, missing);
            if (spent != missing)
                throw new InvalidOperationException(
                    $"Settlement storage spent {spent} of required production input {missing} for resource id {amount.Id.Value}.");

            var added = ProductionStationResourceAccess.AddInput(station, amount.Id, spent);
            if (added != spent)
                throw new InvalidOperationException(
                    $"Production station input accepted {added} of spent {spent} for resource id {amount.Id.Value}.");
        }

        private static void SpendInputs(SW.Entity station, ResourceAmount[] amounts)
        {
            for (var i = 0; i < amounts.Length; i++)
                SpendInput(station, amounts[i]);
        }

        private static void SpendInput(SW.Entity station, ResourceAmount amount)
        {
            var spent = ProductionStationResourceAccess.SpendInput(station, amount.Id, amount.Amount);
            if (spent != amount.Amount)
                throw new InvalidOperationException(
                    $"Production station spent {spent} of required input {amount.Amount} for resource id {amount.Id.Value}.");
        }

        private static int TotalAmount(ResourceAmount[] amounts)
        {
            var total = 0;
            for (var i = 0; i < amounts.Length; i++)
                total += amounts[i].Amount;

            return total;
        }
    }
}
