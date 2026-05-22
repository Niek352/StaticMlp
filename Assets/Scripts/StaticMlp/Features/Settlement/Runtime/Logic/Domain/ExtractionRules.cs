using System;
using StaticMlp.Features.BuildingCatalog;

namespace StaticMlp.Features.Settlement
{
    public static class ExtractionRules
    {
        public static bool TryGetOutputResource(BuildingId buildingId, out ResourceId resourceId)
        {
            var definition = BuildingCatalogData.Get(buildingId);
            resourceId = definition.Operation.OutputResourceId;
            return resourceId.Value != 0;
        }

        public static int FillBuffer(ref ExtractionOperationState state)
        {
            if (!state.Enabled || state.WorkerSlotCount == 0)
                return 0;

            return AddToBuffer(ref state, state.WorkerSlotCount);
        }

        public static int AddToBuffer(ref ExtractionOperationState state, int requestedAmount)
        {
            if (requestedAmount < 0)
                throw new InvalidOperationException($"Cannot add negative extraction output amount {requestedAmount}.");

            var accepted = ClampToBufferCapacity(state.OutputBufferCapacity, state.OutputBufferAmount, requestedAmount);
            state.OutputBufferAmount += accepted;
            return accepted;
        }

        public static int RemoveFromBuffer(ref ExtractionOperationState state, ResourceId resourceId, int requestedAmount)
        {
            if (resourceId != state.OutputResource)
            {
                throw new InvalidOperationException(
                    $"Extraction buffer contains resource id {state.OutputResourceId}, not requested resource id {resourceId.Value}.");
            }

            if (requestedAmount < 0)
                throw new InvalidOperationException($"Cannot remove negative extraction output amount {requestedAmount}.");

            var removed = Math.Min(requestedAmount, state.OutputBufferAmount);
            state.OutputBufferAmount -= removed;
            return removed;
        }

        public static int ClampToBufferCapacity(int capacity, int currentAmount, int requestedAmount)
        {
            if (requestedAmount <= 0)
                return 0;

            var remaining = Math.Max(0, capacity - currentAmount);
            return Math.Min(requestedAmount, remaining);
        }

        public static bool HasOutput(in ExtractionOperationState state)
        {
            return state.Enabled && state.OutputBufferAmount > 0;
        }
    }
}
