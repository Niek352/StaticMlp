using System;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Buildings
{
    public static class ConstructionRules
    {
        public static bool CanBuild(SW.Entity site, in ConstructionSiteState state)
        {
            return ConstructionResourcesAccess.IsComplete(site)
                   && state.Phase is ConstructionPhase.ReadyToBuild or ConstructionPhase.BuildingInProgress;
        }

        public static bool ApplyBuildWork(
            SW.Entity site,
            ref ConstructionSiteState state,
            ref ConstructionProgress progress,
            float requestedWork,
            float maxWork)
        {
            if (!CanBuild(site, in state))
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
