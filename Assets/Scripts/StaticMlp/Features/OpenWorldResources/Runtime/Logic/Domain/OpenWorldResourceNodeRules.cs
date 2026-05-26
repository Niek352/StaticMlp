using System;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.OpenWorldResources
{
    public static class OpenWorldResourceNodeRules
    {
        public static int StartingAmount(ResourcePlacementKindId kindId)
        {
            return OpenWorldResourceHarvestCatalog.Get(kindId).StartingAmount;
        }

        internal static bool IsDepleted(OpenWorldResourceOverlayState state)
        {
            return (state.Flags & OpenWorldResourceOverlayFlags.Depleted) != 0;
        }

        internal static OpenWorldResourceOverlayState ApplyHit(
            ResourcePlacement placement,
            OpenWorldResourceOverlayState currentState,
            out ResourceAmount harvestedResource,
            out bool wasDepleted)
        {
            if (currentState.PlacementId != placement.PlacementId)
                throw new InvalidOperationException($"Overlay state placement {currentState.PlacementId} does not match hit placement {placement.PlacementId}.");
            if (currentState.KindIdValue != 0 && currentState.KindIdValue != placement.KindId.Value)
                throw new InvalidOperationException($"Overlay state kind {currentState.KindIdValue} does not match placement {placement.PlacementId} kind {placement.KindId.Value}.");
            if (IsDepleted(currentState))
                throw new InvalidOperationException($"Cannot apply hit to depleted resource placement {placement.PlacementId}.");
            if (currentState.RemainingAmount == 0)
                throw new InvalidOperationException($"Resource placement {placement.PlacementId} has zero remaining amount without the depleted flag.");

            ref readonly var definition = ref OpenWorldResourceHarvestCatalog.Get(placement.KindId);
            var amountRemoved = Math.Min(definition.DamagePerHit, currentState.RemainingAmount);
            var remainingAmount = currentState.RemainingAmount - amountRemoved;
            wasDepleted = remainingAmount == 0;
            harvestedResource = new ResourceAmount(
                definition.HarvestPerHit.Id,
                Math.Min(definition.HarvestPerHit.Amount, amountRemoved));

            currentState.KindIdValue = placement.KindId.Value;
            currentState.RemainingAmount = checked((ushort)remainingAmount);
            if (wasDepleted)
                currentState.Flags |= OpenWorldResourceOverlayFlags.Depleted;

            return currentState;
        }
    }
}
