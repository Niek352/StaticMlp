using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class ServerTransferExtractionOutputToStockpileSystem : ISystem
    {
        private EventReceiver<ServerWT, TransferExtractionOutputToStockpileEvent> _requests;

        public void Init()
        {
            _requests = SW.RegisterEventReceiver<TransferExtractionOutputToStockpileEvent>();
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

        private static void Handle(in TransferExtractionOutputToStockpileEvent request)
        {
            if (!request.ExtractionBuilding.TryUnpack<ServerWT>(out var extractionBuilding))
                throw new InvalidOperationException($"Extraction transfer target {request.ExtractionBuilding} is not a server entity.");

            ref var extraction = ref extractionBuilding.Mut<ExtractionOperationState>();
            if (!ExtractionRules.HasOutput(in extraction))
                return;

            if (request.Resource != extraction.OutputResource)
            {
                throw new InvalidOperationException(
                    $"Extraction transfer requested resource id {request.Resource.Value}, but buffer contains resource id {extraction.OutputResourceId}.");
            }

            var storageEntity = SettlementSharedResourcesQuery.GetServerEntity();
            var accepted = SettlementSharedResourcesAccess.Add(storageEntity, request.Resource, request.RequestedAmount);
            ExtractionRules.RemoveFromBuffer(ref extraction, request.Resource, accepted);
        }
    }
}
