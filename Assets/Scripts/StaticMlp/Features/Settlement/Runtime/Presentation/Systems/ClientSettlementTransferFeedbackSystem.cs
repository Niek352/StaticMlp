using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public sealed class ClientSettlementTransferFeedbackSystem : ISystem
    {
        private EventReceiver<ClientCoreWT, NetworkEventFromServer<DepositCarriedResourcesToStockpileResultEvent>> _depositResults;
        private EventReceiver<ClientCoreWT, NetworkEventFromServer<ClaimProductionOutputResultEvent>> _claimResults;

        public void Init()
        {
            _depositResults = CW.RegisterEventReceiver<NetworkEventFromServer<DepositCarriedResourcesToStockpileResultEvent>>();
            _claimResults = CW.RegisterEventReceiver<NetworkEventFromServer<ClaimProductionOutputResultEvent>>();
        }

        public void Destroy()
        {
            CW.DeleteEventReceiver(ref _depositResults);
            CW.DeleteEventReceiver(ref _claimResults);
        }

        public void Update()
        {
            ref var feedback = ref CW.GetResource<SettlementTransferFeedbackState>();

            foreach (var evt in _depositResults)
                ApplyDepositResult(ref feedback, in evt.Value.Value);

            foreach (var evt in _claimResults)
                ApplyClaimResult(ref feedback, in evt.Value.Value);
        }

        private static void ApplyDepositResult(
            ref SettlementTransferFeedbackState feedback,
            in DepositCarriedResourcesToStockpileResultEvent result)
        {
            if (result.Status == RequestStatus.Accepted && result.TransferredAmount > 0)
            {
                feedback.Set(result.Building, $"Stored {result.TransferredAmount} resources");
                return;
            }

            feedback.Set(result.Building, IsSharedStorageFull() ? "Storage full" : "No resources stored");
        }

        private static void ApplyClaimResult(
            ref SettlementTransferFeedbackState feedback,
            in ClaimProductionOutputResultEvent result)
        {
            if (result.Status == RequestStatus.Accepted && result.TransferredAmount > 0)
            {
                feedback.Set(
                    result.Building,
                    $"Claimed {result.TransferredAmount} {ResourceCatalog.Get(result.Resource).DisplayName}");
                return;
            }

            feedback.Set(result.Building, IsSharedStorageFull() ? "Storage full" : "No output claimed");
        }

        private static bool IsSharedStorageFull()
        {
            var storageEntity = SettlementSharedResourcesQuery.GetClientEntity();
            ref readonly var storage = ref ClientProjection.Read<SettlementSharedResources>(storageEntity);
            return !StockpileRules.HasAvailableCapacity(
                storage.Capacity,
                SettlementSharedResourcesAccess.TotalProjectedUsed(storageEntity));
        }
    }
}
