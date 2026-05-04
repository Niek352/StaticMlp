using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components.Buildings;
using StaticMlp.Networking;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Buildings
{
    public sealed class ServerBuildConstructionSystem : ISystem
    {
        private readonly float _interactionRange;
        private readonly float _maxWorkPerRequest;
        private EventReceiver<ServerWT, NetworkEventFromClient<BuildConstructionRequestEvent>> _requests;

        public ServerBuildConstructionSystem(float interactionRange = 4f, float maxWorkPerRequest = 5f)
        {
            _interactionRange = interactionRange;
            _maxWorkPerRequest = maxWorkPerRequest;
        }

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<NetworkEventFromClient<BuildConstructionRequestEvent>>();
        }

        public void Destroy()
        {
            SW.DeleteEventReceiver(ref _requests);
        }

        public void Update()
        {
            foreach (var evt in _requests)
            {
                var request = evt.Value;
                Handle(in request);
            }
        }

        private void Handle(in NetworkEventFromClient<BuildConstructionRequestEvent> request)
        {
            var sourcePeer = request.SourcePeer;
            if (!ConstructionSiteQuery.TryGetBuildableSite(request.Value.Site, out var site))
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
                request.Value.WorkAmount,
                _maxWorkPerRequest);
        }
    }
}
