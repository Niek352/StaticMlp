namespace StaticMlp.Features.CampFlow
{
    public interface IFlowObjectiveOverride
    {
        bool TryOverride(in CampFlowContext context, out string objectiveDisplayName, out string hintDisplayName);
    }
}
