using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Frontier
{
    public sealed class ServerFrontierExpeditionResolutionSystem : ISystem
    {
        public void Update()
        {
            foreach (var anchor in SW.Query<All<Stage1SettlementProgression, ActiveExpeditionState>>().Entities())
            {
                ref readonly var activeExpedition = ref anchor.Read<ActiveExpeditionState>();
                if (activeExpedition.Status != ExpeditionActivityStatus.Active)
                    continue;

                var anchorId = anchor.Read<Stage1SettlementProgression>().Anchor;
                if (FrontierEncounterParticipantQuery.HasAnyLivingParticipant(
                        anchorId,
                        FrontierEncounterKind.Expedition,
                        activeExpedition.ExpeditionIdValue))
                {
                    continue;
                }

                ref var mutableExpedition = ref ReplicationMut.Mut<ActiveExpeditionState>(anchor);
                mutableExpedition.Status = ExpeditionActivityStatus.Cleared;
                SW.SendEvent(new ExpeditionRewardGrantedEvent(
                    anchorId,
                    ResolveRewardPackage(activeExpedition.ExpeditionId)));
            }
        }

        private static RewardPackageId ResolveRewardPackage(ExpeditionId expeditionId)
        {
            if (expeditionId == ExpeditionCatalog.NearbyRaiderCampId)
                return RewardPackageCatalog.RecoveredWarCacheId;

            throw new System.InvalidOperationException(
                $"Missing reward package mapping for expedition {expeditionId.Value}.");
        }
    }
}
