using System;
using StaticMlp.Features.Effects;
using StaticMlp.Features.OpenWorldGeneration;

namespace StaticMlp.Features.OpenWorldResources
{
    internal readonly struct OpenWorldResourceHazardDefinition
    {
        public readonly ResourcePlacementKindId KindId;
        public readonly float Radius;
        public readonly float Damage;
        public readonly DamageType DamageType;

        public OpenWorldResourceHazardDefinition(
            ResourcePlacementKindId kindId,
            float radius,
            float damage,
            DamageType damageType)
        {
            if (kindId.Value == 0)
                throw new ArgumentOutOfRangeException(nameof(kindId), kindId.Value, "Resource placement kind id must be non-zero.");
            if (float.IsNaN(radius) || float.IsInfinity(radius) || radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(radius), radius, "Hazard radius must be finite and positive.");
            if (float.IsNaN(damage) || float.IsInfinity(damage) || damage <= 0f)
                throw new ArgumentOutOfRangeException(nameof(damage), damage, "Hazard damage must be finite and positive.");

            KindId = kindId;
            Radius = radius;
            Damage = damage;
            DamageType = damageType;
        }
    }
}
