using System;
using StaticMlp.Features.OpenWorldGeneration;

namespace StaticMlp.Features.CombatDirector
{
    public static class SpawnSourcePlacementRules
    {
        private static readonly SpawnPlacementKindId WILDLIFE_SPAWN = new(1);
        private static readonly SpawnPlacementKindId HIGHLAND_SPAWN = new(2);
        private const float BASE_SOURCE_RADIUS = 5f;

        public static SpawnSourceType ResolveSourceType(SpawnPlacementKindId kindId)
        {
            if (kindId == WILDLIFE_SPAWN)
                return SpawnSourceType.Burrow;

            if (kindId == HIGHLAND_SPAWN)
                return SpawnSourceType.Rift;

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
