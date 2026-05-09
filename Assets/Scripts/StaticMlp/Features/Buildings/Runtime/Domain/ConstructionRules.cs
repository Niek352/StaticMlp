using System;
using StaticMlp.Game.Components.Buildings;

namespace StaticMlp.Features.Buildings
{
    public static class ConstructionRules
    {
        public static bool CanDepositResources(in ConstructionSiteState state)
        {
            return state.Phase is ConstructionPhase.WaitingForResources;
        }

        public static bool TryPlanResourceDeposit(
            in ConstructionSiteState state,
            in ConstructionResources resources,
            int availableWood,
            int availableStone,
            int requestedWood,
            int requestedStone,
            out int wood,
            out int stone)
        {
            if (!CanDepositResources(in state))
            {
                wood = 0;
                stone = 0;
                return false;
            }

            wood = Math.Min(Math.Max(0, requestedWood), Math.Min(resources.RemainingWood, Math.Max(0, availableWood)));
            stone = Math.Min(Math.Max(0, requestedStone), Math.Min(resources.RemainingStone, Math.Max(0, availableStone)));
            return wood > 0 || stone > 0 || resources.IsComplete;
        }

        public static bool ApplyResourceDeposit(
            ref ConstructionSiteState state,
            ref ConstructionResources resources,
            int wood,
            int stone)
        {
            if (!CanDepositResources(in state))
                return false;

            resources.WoodDelivered += Math.Min(Math.Max(0, wood), resources.RemainingWood);
            resources.StoneDelivered += Math.Min(Math.Max(0, stone), resources.RemainingStone);

            if (resources.IsComplete)
                state.Phase = ConstructionPhase.ReadyToBuild;

            return wood > 0 || stone > 0 || resources.IsComplete;
        }

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
