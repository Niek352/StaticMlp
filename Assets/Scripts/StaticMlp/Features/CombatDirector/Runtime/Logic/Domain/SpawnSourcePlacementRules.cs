using System;
using StaticMlp.Features.OpenWorldGeneration;
using Unity.Mathematics;

namespace StaticMlp.Features.CombatDirector
{
    public static class SpawnSourcePlacementRules
    {
        private static readonly SpawnPlacementKindId WILDLIFE_SPAWN = new(1);
        private static readonly SpawnPlacementKindId HIGHLAND_SPAWN = new(2);
        private const float BASE_SOURCE_RADIUS = 5f;

        public static SpawnSource CreateSource(SpawnPlacementKindId kindId, float3 position, float placementScale)
        {
            var radius = ResolveSourceRadius(placementScale);

            if (kindId == WILDLIFE_SPAWN)
            {
                return new SpawnSource
                {
                    Type = SpawnSourceType.Burrow,
                    Kind = SpawnSourceKind.AmbientPoint,
                    Position = position,
                    Radius = radius,
                    IsActive = true,
                    AllowsAmbient = true,
                    AllowsEscalation = false,
                    AllowsPressureEvent = false
                };
            }

            if (kindId == HIGHLAND_SPAWN)
            {
                return new SpawnSource
                {
                    Type = SpawnSourceType.Rift,
                    Kind = SpawnSourceKind.Rift,
                    Position = position,
                    Radius = radius,
                    IsActive = true,
                    AllowsAmbient = false,
                    AllowsEscalation = true,
                    AllowsPressureEvent = true
                };
            }

            throw new InvalidOperationException($"Unknown open-world spawn placement kind: {kindId.Value}.");
        }

        public static float ResolveSourceRadius(float placementScale)
        {
            if (placementScale <= 0f)
                throw new InvalidOperationException("Open-world spawn placement scale must be positive.");

            return BASE_SOURCE_RADIUS * placementScale;
        }
    }
}
