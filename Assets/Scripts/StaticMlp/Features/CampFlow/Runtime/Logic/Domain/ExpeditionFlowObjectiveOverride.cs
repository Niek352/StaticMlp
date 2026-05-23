using StaticMlp.Features.Frontier;

namespace StaticMlp.Features.CampFlow
{
    public readonly struct ExpeditionFlowObjectiveOverride : IFlowObjectiveOverride
    {
        public bool TryOverride(
            in CampFlowContext context,
            out string objectiveDisplayName,
            out string hintDisplayName)
        {
            hintDisplayName = string.Empty;

            if (context.Expedition.Status == ExpeditionActivityStatus.Active)
            {
                objectiveDisplayName = CampFlowCatalog.CLEAR_EXPEDITION_OBJECTIVE;
                return true;
            }

            if (context.Availability.Status == ExpeditionAvailabilityStatus.Available)
            {
                objectiveDisplayName = CampFlowCatalog.START_EXPEDITION_OBJECTIVE;
                return true;
            }

            objectiveDisplayName = string.Empty;
            return false;
        }
    }
}
