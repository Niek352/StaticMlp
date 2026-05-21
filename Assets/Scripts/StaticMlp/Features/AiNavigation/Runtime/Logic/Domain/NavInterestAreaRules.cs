using System;
using StaticMlp.Features.CombatDirector;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public static class NavInterestAreaRules
    {
        private const float NAV_BUILD_RADIUS_MULTIPLIER = 1.6f;
        private const float SOURCE_COLLECT_RADIUS_MULTIPLIER = 2f;
        private const float NAV_SECTOR_SIZE = 16f;
        private const int PRIORITY_BASE = 1000;
        private const float PRIORITY_RADIUS_WEIGHT = 10f;

        public static NavInterestArea CreateFromCombatCell(in CombatCell combatCell)
        {
            Validate(in combatCell);

            var navBuildRadius = math.max(combatCell.Radius, combatCell.Radius * NAV_BUILD_RADIUS_MULTIPLIER);
            var sourceCollectRadius = math.max(combatCell.Radius * NAV_BUILD_RADIUS_MULTIPLIER, combatCell.Radius * SOURCE_COLLECT_RADIUS_MULTIPLIER);

            return new NavInterestArea
            {
                AreaId = combatCell.CellId,
                Kind = NavInterestAreaKind.CombatCell,
                Center = combatCell.Center,
                Radius = combatCell.Radius,
                NavBuildRadius = navBuildRadius,
                SourceCollectRadius = sourceCollectRadius,
                Priority = checked(PRIORITY_BASE + (int)math.ceil(combatCell.Radius * PRIORITY_RADIUS_WEIGHT))
            };
        }

        public static float3 QuantizeNavCenter(float3 center)
        {
            if (!math.all(math.isfinite(center)))
                throw new InvalidOperationException("Nav area center must be finite before quantization.");

            return new float3(
                math.round(center.x / NAV_SECTOR_SIZE) * NAV_SECTOR_SIZE,
                center.y,
                math.round(center.z / NAV_SECTOR_SIZE) * NAV_SECTOR_SIZE);
        }

        private static void Validate(in CombatCell combatCell)
        {
            if (!math.all(math.isfinite(combatCell.Center)))
                throw new InvalidOperationException($"CombatCell {combatCell.CellId} has a non-finite center.");

            if (!math.isfinite(combatCell.Radius) || combatCell.Radius <= 0f)
                throw new InvalidOperationException($"CombatCell {combatCell.CellId} has an invalid radius {combatCell.Radius}.");
        }
    }
}
