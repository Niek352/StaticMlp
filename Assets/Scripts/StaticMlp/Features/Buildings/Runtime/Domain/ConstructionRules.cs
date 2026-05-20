using System;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Buildings
{
    public static class ConstructionRules
    {
        public static bool CanBuild(in ConstructionSiteState state, in ConstructionResources resources)
        {
            return resources.IsComplete && state.Phase is ConstructionPhase.ReadyToBuild or ConstructionPhase.BuildingInProgress;
        }

        public static bool ApplyBuildWork(
            ref ConstructionSiteState state,
            ref ConstructionProgress progress,
            in ConstructionResources resources,
            float requestedWork,
            float maxWork)
        {
            if (!CanBuild(in state, in resources))
                return false;

            var work = Math.Min(Math.Max(0f, requestedWork), maxWork);
            if (work <= 0f)
                return false;

            progress.BuildWorkDone = Math.Min(progress.BuildWorkRequired, progress.BuildWorkDone + work);
            state.Phase = progress.IsComplete
                ? ConstructionPhase.Completed
                : ConstructionPhase.BuildingInProgress;
            return true;
        }
    }
}
