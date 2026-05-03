using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Game.Systems;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Buildings
{
    public sealed class ServerPlaceBuildingRequestSystem : ISystem
    {
        public void Update()
        {
            ref var inbox = ref SW.GetResource<NetInbox>();

            foreach (var evt in inbox.Events)
            {
                if (evt.EventTypeId != GameplayEventTypeIds.PlaceBuildingRequest)
                    continue;

                if (!ConstructionEventCodec.TryReadPlaceBuilding(evt.Payload, out var request))
                    continue;

                if (!ServerConstructionAuthorization.HasPlayer(evt.SourcePeer))
                    continue;

                var id = new BuildingId(request.BuildingId);
                if (!StaticMlp.Features.BuildingCatalog.BuildingCatalog.TryGetDefinition(id, out var definition))
                    continue;

                var validation = ConstructionPlacementValidator.ValidateServer(
                    definition,
                    request.Position,
                    request.Rotation);
                if (!validation.IsValid)
                    continue;

                ServerBuildingSpawns.SpawnConstructionSite(
                    evt.SourcePeer,
                    definition,
                    request.Position,
                    request.Rotation);
            }
        }
    }
}
