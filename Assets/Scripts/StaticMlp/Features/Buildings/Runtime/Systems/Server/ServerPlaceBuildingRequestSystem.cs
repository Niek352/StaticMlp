using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Buildings
{
    public sealed class ServerPlaceBuildingRequestSystem : ISystem
    {
        private EventReceiver<ServerWT, NetworkEventFromClient<PlaceBuildingRequestEvent>> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<NetworkEventFromClient<PlaceBuildingRequestEvent>>();
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

        private static void Handle(in NetworkEventFromClient<PlaceBuildingRequestEvent> request)
        {
            var sourcePeer = request.SourcePeer;
            if (!ServerPeerPlayers.HasPlayer(sourcePeer))
                return;

            var id = new BuildingId(request.Value.BuildingId);
            var definition = BuildingCatalogData.Get(id);

            var validation = ConstructionPlacementValidator.ValidateAuthoritative(
                definition,
                request.Value.Position,
                request.Value.Rotation);
            if (!validation.IsValid)
                return;

            BuildingEntitySpawner.SpawnConstructionSite(new ConstructionSiteSpawnSpec(
                sourcePeer,
                definition,
                SettlementAnchorCatalog.HomeCampId,
                request.Value.Position,
                request.Value.Rotation,
                startReadyToBuild: false,
                initialBuildWork: 0f));
        }
    }
}
