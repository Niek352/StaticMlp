using System;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Buildings
{
    public sealed class DepositConstructionResourcesHandler
        : IRequestHandler<DepositConstructionResourcesRequestEvent, DepositConstructionResourcesResultEvent>
    {
        private readonly float _interactionRange;

        public DepositConstructionResourcesHandler(float interactionRange = 4f)
        {
            _interactionRange = interactionRange;
        }

        public DepositConstructionResourcesResultEvent Handle(
            NetworkPeerId sourcePeer,
            in DepositConstructionResourcesRequestEvent request)
        {
            var rejected = new DepositConstructionResourcesResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Rejected,
                Site = request.Site,
                AcceptedResources = Array.Empty<ResourceAmount>()
            };

            if (!ConstructionSiteQuery.TryGetConstructionSite(request.Site, out var site))
                return rejected;

            var transform = site.Read<ConstructionTransform>();
            if (!ServerPeerPlayers.IsPlayerNear(sourcePeer, transform.Position, _interactionRange))
                return rejected;

            if (!HasValidAmounts(request.Resources))
                return rejected;

            var storageEntity = SettlementSharedResourcesQuery.GetServerEntity();
            var currentState = site.Read<ConstructionSiteState>();
            if (!SettlementConstructionRules.TryPlanResourceDeposit(
                    in currentState,
                    site,
                    storageEntity,
                    request.Resources,
                    out var acceptedResources))
                return rejected;

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

            return new DepositConstructionResourcesResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Accepted,
                Site = request.Site,
                AcceptedResources = spentResources
            };
        }

        private static bool HasValidAmounts(ResourceAmount[] resources)
        {
            if (resources == null)
                return false;

            for (var i = 0; i < resources.Length; i++)
            {
                if (resources[i].Amount < 0)
                    return false;
            }

            return true;
        }
    }
}
