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
        public readonly int Armor;
        public readonly int ResistancePercent;
        public readonly OpenWorldResourceHarvestTag PreferredHarvestTags;

        public OpenWorldResourceHarvestDefinition(
            ResourcePlacementKindId kindId,
            int startingAmount,
            int damagePerHit,
            ResourceAmount harvestPerHit,
            int armor,
            int resistancePercent,
            OpenWorldResourceHarvestTag preferredHarvestTags)
        {
            if (kindId.Value == 0)
                throw new ArgumentOutOfRangeException(nameof(kindId), kindId.Value, "Resource placement kind id must be non-zero.");
            if (startingAmount <= 0)
                throw new ArgumentOutOfRangeException(nameof(startingAmount), startingAmount, "Starting amount must be positive.");
            if (damagePerHit <= 0)
                throw new ArgumentOutOfRangeException(nameof(damagePerHit), damagePerHit, "Damage per hit must be positive.");
            if (harvestPerHit.Amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(harvestPerHit), harvestPerHit.Amount, "Harvest amount per hit must be positive.");
            if (armor < 0)
                throw new ArgumentOutOfRangeException(nameof(armor), armor, "Armor must be zero or positive.");
            if (resistancePercent < 0 || resistancePercent > 100)
                throw new ArgumentOutOfRangeException(nameof(resistancePercent), resistancePercent, "Resistance percent must be between 0 and 100.");
            if (preferredHarvestTags == OpenWorldResourceHarvestTag.None)
                throw new ArgumentOutOfRangeException(nameof(preferredHarvestTags), preferredHarvestTags, "Preferred harvest tags must not be None.");

            KindId = kindId;
            StartingAmount = startingAmount;
            DamagePerHit = damagePerHit;
            HarvestPerHit = harvestPerHit;
            Armor = armor;
            ResistancePercent = resistancePercent;
            PreferredHarvestTags = preferredHarvestTags;
        }
    }
}
