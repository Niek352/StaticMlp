using System;

namespace StaticMlp.Features.Settlement
{
    public static class SettlementConstructionRules
    {
        public static bool CanDepositResources(in ConstructionSiteState state)
        {
            return state.Phase is ConstructionPhase.WaitingForResources;
        }

        public static ConstructionResources CreateResources(ResourceAmount[] constructionCost)
        {
            var resources = new ConstructionResources();

            for (var i = 0; i < constructionCost.Length; i++)
            {
                var cost = constructionCost[i];
                resources.SetRequired(cost.Id, cost.Amount);
            }

            return resources;
        }

        public static void MarkAllResourcesDelivered(ref ConstructionResources resources)
        {
            resources.MarkDelivered(ResourceCatalog.WoodId);
            resources.MarkDelivered(ResourceCatalog.StoneId);
            resources.MarkDelivered(ResourceCatalog.PlanksId);
            resources.MarkDelivered(ResourceCatalog.SimplePartsId);
        }

        public static bool TryPlanResourceDeposit(
            in ConstructionSiteState state,
            in ConstructionResources resources,
            int availableWood,
            int availableStone,
            int availablePlanks,
            int availableSimpleParts,
            int requestedWood,
            int requestedStone,
            int requestedPlanks,
            int requestedSimpleParts,
            out int wood,
            out int stone,
            out int planks,
            out int simpleParts)
        {
            if (!CanDepositResources(in state))
            {
                wood = 0;
                stone = 0;
                planks = 0;
                simpleParts = 0;
                return false;
            }

            wood = PlanResourceDeposit(resources.RemainingWood, availableWood, requestedWood);
            stone = PlanResourceDeposit(resources.RemainingStone, availableStone, requestedStone);
            planks = PlanResourceDeposit(resources.RemainingPlanks, availablePlanks, requestedPlanks);
            simpleParts = PlanResourceDeposit(resources.RemainingSimpleParts, availableSimpleParts, requestedSimpleParts);
            return wood > 0 || stone > 0 || planks > 0 || simpleParts > 0 || resources.IsComplete;
        }

        public static bool ApplyResourceDeposit(
            ref ConstructionSiteState state,
            ref ConstructionResources resources,
            int wood,
            int stone,
            int planks,
            int simpleParts)
        {
            if (!CanDepositResources(in state))
                return false;

            var deliveredWood = resources.Deliver(ResourceCatalog.WoodId, wood);
            var deliveredStone = resources.Deliver(ResourceCatalog.StoneId, stone);
            var deliveredPlanks = resources.Deliver(ResourceCatalog.PlanksId, planks);
            var deliveredSimpleParts = resources.Deliver(ResourceCatalog.SimplePartsId, simpleParts);

            if (resources.IsComplete)
                state.Phase = ConstructionPhase.ReadyToBuild;

            return deliveredWood > 0
                   || deliveredStone > 0
                   || deliveredPlanks > 0
                   || deliveredSimpleParts > 0
                   || resources.IsComplete;
        }

        private static int PlanResourceDeposit(int remaining, int available, int requested)
        {
            return Math.Min(Math.Max(0, requested), Math.Min(remaining, Math.Max(0, available)));
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
