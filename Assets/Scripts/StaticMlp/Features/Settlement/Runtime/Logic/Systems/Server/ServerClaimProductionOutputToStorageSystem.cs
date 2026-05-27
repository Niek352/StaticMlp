using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class ServerClaimProductionOutputToStorageSystem : ISystem
    {
        private EventReceiver<ServerWT, ClaimProductionOutputToStorageEvent> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<ClaimProductionOutputToStorageEvent>();
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

        private static void Handle(in ClaimProductionOutputToStorageEvent request)
        {
            if (!request.ProductionBuilding.TryUnpack<ServerWT>(out var station))
                throw new InvalidOperationException(
                    $"Production output claim target {request.ProductionBuilding} is not a server entity.");

            ref readonly var state = ref station.Read<ProductionStationOperationState>();
            if (!ProductionStationRules.IsStationOutputResource(state.Station, request.Resource))
            {
                throw new InvalidOperationException(
                    $"Production station id {state.StationId} cannot output resource id {request.Resource.Value}.");
            }

            var availableOutput = ProductionStationResourceAccess.GetOutput(station, request.Resource);
            if (availableOutput <= 0)
                return;

            var storageEntity = SettlementSharedResourcesQuery.GetServerEntity();
            var accepted = SettlementSharedResourcesAccess.Add(
                storageEntity,
                request.Resource,
                Math.Min(request.RequestedAmount, availableOutput));
            if (accepted <= 0)
                return;

            var removed = ProductionStationResourceAccess.RemoveOutput(station, request.Resource, accepted);
            if (removed != accepted)
            {
                throw new InvalidOperationException(
                    $"Production station removed {removed} of accepted output claim {accepted} for resource id {request.Resource.Value}.");
            }
        }
    }
}
