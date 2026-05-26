using System;
using StaticMlp.Features.OpenWorldGeneration;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.OpenWorldResources
{
    internal readonly struct OpenWorldResourceHarvestDefinition
    {
        public readonly ResourcePlacementKindId KindId;
        public readonly int StartingAmount;
        public readonly int DamagePerHit;
        public readonly ResourceAmount HarvestPerHit;

        public OpenWorldResourceHarvestDefinition(
            ResourcePlacementKindId kindId,
            int startingAmount,
            int damagePerHit,
            ResourceAmount harvestPerHit)
        {
            if (kindId.Value == 0)
                throw new ArgumentOutOfRangeException(nameof(kindId), kindId.Value, "Resource placement kind id must be non-zero.");
            if (startingAmount <= 0)
                throw new ArgumentOutOfRangeException(nameof(startingAmount), startingAmount, "Starting amount must be positive.");
            if (damagePerHit <= 0)
                throw new ArgumentOutOfRangeException(nameof(damagePerHit), damagePerHit, "Damage per hit must be positive.");
            if (harvestPerHit.Amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(harvestPerHit), harvestPerHit.Amount, "Harvest amount per hit must be positive.");

            KindId = kindId;
            StartingAmount = startingAmount;
            DamagePerHit = damagePerHit;
            HarvestPerHit = harvestPerHit;
        }
    }
}
