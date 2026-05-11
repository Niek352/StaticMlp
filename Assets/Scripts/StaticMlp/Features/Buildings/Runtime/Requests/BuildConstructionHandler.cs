using StaticMlp.Features.Settlement;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Buildings
{
    public sealed class BuildConstructionHandler
        : IRequestHandler<BuildConstructionRequestEvent, BuildConstructionResultEvent>
    {
        private readonly float _interactionRange;
        private readonly float _maxWorkPerRequest;

        public BuildConstructionHandler(float interactionRange = 4f, float maxWorkPerRequest = 35f)
        {
            _interactionRange = interactionRange;
            _maxWorkPerRequest = maxWorkPerRequest;
        }

        public BuildConstructionResultEvent Handle(NetworkPeerId sourcePeer, in BuildConstructionRequestEvent request)
        {
            var rejected = new BuildConstructionResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Rejected,
                Site = request.Site
            };

            if (!ConstructionSiteQuery.TryGetBuildableSite(request.Site, out var site))
                return rejected;

            var transform = site.Read<ConstructionTransform>();
            if (!ServerPeerPlayers.IsPlayerNear(sourcePeer, transform.Position, _interactionRange))
                return rejected;

            ref var state = ref ReplicationMut.Mut<ConstructionSiteState>(site);
            ref readonly var resources = ref site.Read<ConstructionResources>();
            ref var progress = ref ReplicationMut.Mut<ConstructionProgress>(site);
            var before = progress.BuildWorkDone;
            if (!ConstructionRules.ApplyBuildWork(
                    ref state,
                    ref progress,
                    in resources,
                    request.WorkAmount,
                    _maxWorkPerRequest))
                return rejected;

            return new BuildConstructionResultEvent
            {
                RequestId = request.RequestId,
                Status = RequestStatus.Accepted,
                Site = request.Site,
                AcceptedWork = progress.BuildWorkDone - before,
                BuildWorkDone = progress.BuildWorkDone
            };
        }
    }
}
