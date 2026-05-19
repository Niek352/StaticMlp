using System;
using StaticMlp.Features.Frontier;
using Unity.Mathematics;

namespace StaticMlp.Features.AiNavigation
{
    public static class CombatCellNavAreaRules
    {
        private const float NAV_BUILD_RADIUS_MULTIPLIER = 1.6f;
        private const float SOURCE_COLLECT_RADIUS_MULTIPLIER = 2f;
        private const int PRIORITY_BASE = 1000;
        private const float PRIORITY_RADIUS_WEIGHT = 10f;

        public static CombatCellNavArea Create(in CombatCell combatCell)
        {
            Validate(in combatCell);

            return new CombatCellNavArea
            {
                CellId = combatCell.CellId,
                Center = combatCell.Center,
                Radius = combatCell.Radius,
                NavBuildRadius = math.max(combatCell.Radius, combatCell.Radius * NAV_BUILD_RADIUS_MULTIPLIER),
                SourceCollectRadius = math.max(combatCell.Radius * NAV_BUILD_RADIUS_MULTIPLIER, combatCell.Radius * SOURCE_COLLECT_RADIUS_MULTIPLIER),
                Priority = checked(PRIORITY_BASE + (int)math.ceil(combatCell.Radius * PRIORITY_RADIUS_WEIGHT))
            };
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
