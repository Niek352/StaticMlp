using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Settlement
{
    public sealed class ServerDepositConstructionResourcesSystem : ISystem
    {
        private EventReceiver<ServerWT, DepositConstructionResourcesEvent> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<DepositConstructionResourcesEvent>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _requests);
        }

        public void Update()
        {
            foreach (var request in _requests)
                Handle(in request.Value);
        }

        private static void Handle(in DepositConstructionResourcesEvent request)
        {
            if (!request.Site.TryUnpack<ServerWT>(out var site))
                throw new InvalidOperationException($"Construction resource deposit target {request.Site} is not a server entity.");

            var storageEntity = SettlementSharedResourcesQuery.GetServerEntity();

            ref readonly var currentState = ref site.Read<ConstructionSiteState>();
            if (!SettlementConstructionRules.TryPlanResourceDeposit(
                    in currentState,
                    site,
                    storageEntity,
                    request.Resources,
                    out var acceptedResources))
                return;

            var spentResources = new ResourceAmount[acceptedResources.Length];
            var spentCount = 0;
            for (var i = 0; i < acceptedResources.Length; i++)
            {
                var accepted = acceptedResources[i];
                var spent = SettlementSharedResourcesAccess.Spend(storageEntity, accepted.Id, accepted.Amount);
                if (spent > 0)
                    spentResources[spentCount++] = new ResourceAmount(accepted.Id, spent);
            }

            if (spentCount != spentResources.Length)
            {
                var compact = new ResourceAmount[spentCount];
                Array.Copy(spentResources, compact, spentCount);
                spentResources = compact;
            }

            ref var state = ref ReplicationMut.Mut<ConstructionSiteState>(site);
            SettlementConstructionRules.ApplyResourceDeposit(site, ref state, spentResources);
        }
    }
}
