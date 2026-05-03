using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Game.Systems.Server;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Buildings
{
    public sealed class ServerPlaceBuildingRequestSystem : ISystem
    {
        public void Update()
        {
            NetworkEvents.ForEachServer<PlaceBuildingRequestEvent>(Handle);
        }

        private static void Handle(NetworkPeerId sourcePeer, in PlaceBuildingRequestEvent request)
        {
            if (!ServerPeerPlayers.HasPlayer(sourcePeer))
                return;

            var id = new BuildingId(request.BuildingId);
            if (!StaticMlp.Features.BuildingCatalog.BuildingCatalog.TryGetDefinition(id, out var definition))
                return;

            var validation = ConstructionPlacementValidator.ValidateServer(
                definition,
                request.Position,
                request.Rotation);
            if (!validation.IsValid)
                return;

            ServerBuildingSpawns.SpawnConstructionSite(
                sourcePeer,
                definition,
                request.Position,
                request.Rotation);
        }
    }
}
