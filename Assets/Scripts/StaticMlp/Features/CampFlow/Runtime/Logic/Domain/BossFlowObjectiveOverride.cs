using StaticMlp.Features.Frontier;

namespace StaticMlp.Features.CampFlow
{
    public readonly struct BossFlowObjectiveOverride : IFlowObjectiveOverride
    {
        public bool TryOverride(
            in CampFlowContext context,
            out string objectiveDisplayName,
            out string hintDisplayName)
        {
            hintDisplayName = string.Empty;

            if (context.Boss.Status == BossEncounterStatus.Defeated)
            {
                objectiveDisplayName = CampFlowCatalog.COMPLETE_OBJECTIVE;
                return true;
            }

            if (context.Boss.Status == BossEncounterStatus.Active)
            {
                objectiveDisplayName = CampFlowCatalog.DEFEAT_BOSS_OBJECTIVE;
                return true;
            }

            if (context.Boss.Status == BossEncounterStatus.Available)
            {
                objectiveDisplayName = CampFlowCatalog.START_BOSS_OBJECTIVE;
                return true;
            }

            objectiveDisplayName = string.Empty;
            return false;
        }
    }
}
