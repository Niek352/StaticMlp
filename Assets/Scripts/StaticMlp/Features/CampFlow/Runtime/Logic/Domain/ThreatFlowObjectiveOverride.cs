using StaticMlp.Features.Frontier;

namespace StaticMlp.Features.CampFlow
{
    public readonly struct ThreatFlowObjectiveOverride : IFlowObjectiveOverride
    {
        public bool TryOverride(
            in CampFlowContext context,
            out string objectiveDisplayName,
            out string hintDisplayName)
        {
            if (context.Threat.Phase is ThreatPhase.RaidPending or ThreatPhase.RaidActive)
            {
                objectiveDisplayName = CampFlowCatalog.DEFEND_CAMP_OBJECTIVE;
                hintDisplayName = string.Empty;
                return true;
            }

            objectiveDisplayName = string.Empty;
            hintDisplayName = string.Empty;
            return false;
        }
    }
}
