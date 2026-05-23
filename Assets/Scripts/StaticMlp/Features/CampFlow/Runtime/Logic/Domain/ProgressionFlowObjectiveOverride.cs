using StaticMlp.Features.Progression;

namespace StaticMlp.Features.CampFlow
{
    public readonly struct ProgressionFlowObjectiveOverride : IFlowObjectiveOverride
    {
        public bool TryOverride(
            in CampFlowContext context,
            out string objectiveDisplayName,
            out string hintDisplayName)
        {
            hintDisplayName = string.Empty;

            if (context.Progression.HasFlag(ProgressFlagCatalog.CounterattackDefendedId)
                && !context.Progression.HasFlag(ProgressFlagCatalog.BossUnlockedId))
            {
                objectiveDisplayName = CampFlowCatalog.PREPARE_BOSS_OBJECTIVE;
                return true;
            }

            if (context.Progression.HasFlag(ProgressFlagCatalog.BossUnlockedId))
            {
                objectiveDisplayName = CampFlowCatalog.START_BOSS_OBJECTIVE;
                return true;
            }

            objectiveDisplayName = string.Empty;
            return false;
        }
    }
}
