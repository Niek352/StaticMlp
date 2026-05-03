using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components.Buildings;
using StaticMlp.Networking;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Buildings
{
    public sealed class ServerBuildConstructionSystem : ISystem
    {
        private readonly float _interactionRange;
        private readonly float _maxWorkPerRequest;

        public ServerBuildConstructionSystem(float interactionRange = 4f, float maxWorkPerRequest = 5f)
        {
            _interactionRange = interactionRange;
            _maxWorkPerRequest = maxWorkPerRequest;
        }

        public void Update()
        {
            NetworkEvents.ForEachServer<BuildConstructionRequestEvent>(Handle);
        }

        private void Handle(NetworkPeerId sourcePeer, in BuildConstructionRequestEvent request)
        {
            if (!ConstructionSiteQuery.TryGetServerBuildableSite(request.Site, out var site))
                return;

            if (!NetworkEntityOwnership.IsOwnedBy(site, sourcePeer))
                return;

            var transform = site.Read<ConstructionTransform>();
            if (!ServerPeerPlayers.IsPlayerNear(sourcePeer, transform.Position, _interactionRange))
                return;

            ref var state = ref site.Mut<ConstructionSiteState>();
            ref readonly var resources = ref site.Read<ConstructionResources>();
            ref var progress = ref site.Mut<ConstructionProgress>();
            ConstructionRules.ApplyBuildWork(
                ref state,
                ref progress,
                in resources,
                request.WorkAmount,
                _maxWorkPerRequest);
        }
    }
}
