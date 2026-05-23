using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Game;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.CampFlow
{
    public sealed class CampFlowAnchorFactory : NetEntityFactory<CampFlowAnchorNetworkEntity>, IResource
    {
        public EntityGID Spawn(in CampFlowAnchorSpawnSpec spec)
        {
            if (spec.AnchorId.Value == 0)
                throw new InvalidOperationException("Stage1 camp anchor spawn requires a non-zero settlement anchor id.");

            var entity = CreateEntity(
                new NetworkPeerId(0),
                NetworkAuthority.Server,
                SettlementNetworkArchetypeIds.CampAnchor);
            Configure(entity, spec);
            SendEntity(entity);
            return entity;
        }

        private static void Configure(SW.Entity entity, in CampFlowAnchorSpawnSpec spec)
        {
            entity.Set(new CampFlowProgression
            {
                AnchorId = spec.AnchorId.Value,
                Stage = CampFlowStage.DamagedCampStart
            });
            entity.Set(new CampFlowViewState
            {
                AnchorId = spec.AnchorId.Value,
                Stage = CampFlowStage.DamagedCampStart,
                Objective = Stage1FlowObjective.RepairCamp,
                Hint = Stage1FlowHint.GatherRepairResources
            });
            entity.Set(new ProgressionState(spec.AnchorId, spec.StartingFlagsMask));
            entity.Set(new SettlementAnchorLocation(spec.Position, spec.Rotation));
            entity.Set(new SettlementWorkerSummary
            {
                AnchorId = spec.AnchorId.Value
            });
            entity.Set(new SettlementCampBuilderJobState
            {
                AnchorId = spec.AnchorId.Value
            });
            entity.Set(new ExpeditionAvailabilityState
            {
                ExpeditionIdValue = ExpeditionCatalog.NearbyRaiderCampId.Value,
                Status = ExpeditionAvailabilityStatus.Unavailable
            });
            entity.Set(new ActiveExpeditionState
            {
                Status = ExpeditionActivityStatus.None
            });
            entity.Set(new ThreatState
            {
                Phase = ThreatPhase.Calm
            });
            entity.Set(new RaidScheduleState
            {
                Status = RaidScheduleStatus.None
            });
            entity.Set(new BossEncounterState
            {
                BossIdValue = BossCatalog.RaiderChiefId.Value,
                Status = BossEncounterStatus.Unavailable
            });
            entity.Set(new BossLoadoutPreparationState
            {
                Status = BossLoadoutPreparationStatus.None
            });
            entity.Set(new BossPreparedLoadoutSnapshot());
        }
    }
}
