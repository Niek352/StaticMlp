using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    public static class SettlementConstructionRules
    {
        public static bool CanDepositResources(in ConstructionSiteState state)
        {
            return state.Phase is ConstructionPhase.WaitingForResources;
        }

        public static void MarkAllResourcesDelivered(SW.Entity site)
        {
            ConstructionResourcesAccess.MarkAllDelivered(site);
        }

        public static bool TryPlanResourceDeposit<TWorld>(
            in ConstructionSiteState state,
            World<TWorld>.Entity site,
            World<TWorld>.Entity storage,
            ResourceAmount[] requestedResources,
            out ResourceAmount[] acceptedResources)
            where TWorld : struct, IWorldType
        {
            if (!CanDepositResources(in state))
            {
                acceptedResources = Array.Empty<ResourceAmount>();
                return false;
            }

            acceptedResources = PlanResourceDeposit(
                requestedResources,
                id => ConstructionResourcesAccess.GetRemaining(site, id),
                id => SettlementSharedResourcesAccess.GetAmount(storage, id));
            return acceptedResources.Length > 0 || ConstructionResourcesAccess.IsComplete(site);
        }

        public static bool TryPlanProjectedResourceDeposit(
            in ConstructionSiteState state,
            CW.Entity site,
            CW.Entity storage,
            ResourceAmount[] requestedResources,
            out ResourceAmount[] acceptedResources)
        {
            if (!CanDepositResources(in state))
            {
                acceptedResources = Array.Empty<ResourceAmount>();
                return false;
            }

            acceptedResources = PlanResourceDeposit(
                requestedResources,
                id => ConstructionResourcesAccess.GetProjectedRemaining(site, id),
                id => SettlementSharedResourcesAccess.GetProjectedAmount(storage, id));
            return acceptedResources.Length > 0 || ConstructionResourcesAccess.IsProjectedComplete(site);
        }

        public static bool ApplyResourceDeposit(
            SW.Entity site,
            ref ConstructionSiteState state,
            ResourceAmount[] acceptedResources)
        {
            if (!CanDepositResources(in state))
                return false;

            var deliveredAny = false;
            for (var i = 0; i < acceptedResources.Length; i++)
            {
                var resource = acceptedResources[i];
                if (ConstructionResourcesAccess.Deliver(site, resource.Id, resource.Amount) > 0)
                    deliveredAny = true;
            }

            if (ConstructionResourcesAccess.IsComplete(site))
                state.Phase = ConstructionPhase.ReadyToBuild;

            return deliveredAny || ConstructionResourcesAccess.IsComplete(site);
        }

        public static bool ApplyProjectedResourceDeposit(
            CW.Entity site,
            ref ConstructionSiteState state,
            ResourceAmount[] acceptedResources)
        {
            if (!CanDepositResources(in state))
                return false;

            var deliveredAny = false;
            for (var i = 0; i < acceptedResources.Length; i++)
            {
                var resource = acceptedResources[i];
                if (ConstructionResourcesAccess.DeliverProjected(site, resource.Id, resource.Amount) > 0)
                    deliveredAny = true;
            }

            if (ConstructionResourcesAccess.IsProjectedComplete(site))
                state.Phase = ConstructionPhase.ReadyToBuild;

            return deliveredAny || ConstructionResourcesAccess.IsProjectedComplete(site);
        }

        public static bool CanBuild<TWorld>(in ConstructionSiteState state, World<TWorld>.Entity site)
            where TWorld : struct, IWorldType
        {
            return ConstructionResourcesAccess.IsComplete(site)
                   && state.Phase is ConstructionPhase.ReadyToBuild or ConstructionPhase.BuildingInProgress;
        }

        public static bool CanProjectedBuild(in ConstructionSiteState state, CW.Entity site)
        {
            return ConstructionResourcesAccess.IsProjectedComplete(site)
                   && state.Phase is ConstructionPhase.ReadyToBuild or ConstructionPhase.BuildingInProgress;
        }

        public static bool ApplyBuildWork(
            SW.Entity site,
            ref ConstructionSiteState state,
            ref ConstructionProgress progress,
            float requestedWork,
            float maxWork)
        {
            if (!CanBuild(in state, site))
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

        public static bool ApplyProjectedBuildWork(
            CW.Entity site,
            ref ConstructionSiteState state,
            ref ConstructionProgress progress,
            float requestedWork,
            float maxWork)
        {
            if (!CanProjectedBuild(in state, site))
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

        private static ResourceAmount[] PlanResourceDeposit(
            ResourceAmount[] requestedResources,
            Func<ResourceId, int> remainingFor,
            Func<ResourceId, int> availableFor)
        {
            if (requestedResources == null)
                throw new InvalidOperationException("Construction resource deposit request resources cannot be null.");

            var accepted = new ResourceAmount[requestedResources.Length];
            var acceptedCount = 0;

            for (var i = 0; i < requestedResources.Length; i++)
            {
                var requested = requestedResources[i];
                ValidateRequestedResource(requested);
                for (var j = 0; j < i; j++)
                {
                    if (requestedResources[j].Id == requested.Id)
                        throw new InvalidOperationException($"Duplicate construction resource deposit id {requested.Id.Value}.");
                }

                var amount = PlanResourceDeposit(
                    remainingFor(requested.Id),
                    availableFor(requested.Id),
                    requested.Amount);
                if (amount <= 0)
                    continue;

                accepted[acceptedCount++] = new ResourceAmount(requested.Id, amount);
            }

            if (acceptedCount == accepted.Length)
                return accepted;

            var result = new ResourceAmount[acceptedCount];
            Array.Copy(accepted, result, acceptedCount);
            return result;
        }

        private static void ValidateRequestedResource(ResourceAmount requested)
        {
            ResourceCatalog.Get(requested.Id);
            if (requested.Amount < 0)
                throw new InvalidOperationException($"Construction resource deposit id {requested.Id.Value} has negative amount {requested.Amount}.");
        }

        private static int PlanResourceDeposit(int remaining, int available, int requested)
        {
            return Math.Min(Math.Max(0, requested), Math.Min(remaining, Math.Max(0, available)));
        }
    }
}
