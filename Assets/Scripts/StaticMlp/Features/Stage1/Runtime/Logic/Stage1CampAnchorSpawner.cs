using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Build;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Stage1
{
    public static class Stage1CampAnchorSpawner
    {
        public static EntityGID Spawn(in Stage1CampAnchorSpawnSpec spec)
        {
            if (spec.AnchorId.Value == 0)
                throw new InvalidOperationException("Stage1 camp anchor spawn requires a non-zero settlement anchor id.");

            var localSpec = spec;
            return NetworkEntitySpawner.SpawnServerEntity<Stage1CampAnchorNetworkEntity>(
                new NetworkPeerId(0),
                NetworkAuthority.Server,
                entity => InitializeAnchor(entity, in localSpec));
        }

        private static void InitializeAnchor(SW.Entity entity, in Stage1CampAnchorSpawnSpec spec)
        {
            entity.Set(new Stage1SettlementProgression
            {
                AnchorId = spec.AnchorId.Value,
                Stage = Stage1SettlementProgressStage.DamagedCampStart
            });
            entity.Set(new Stage1FlowViewState
            {
                AnchorId = spec.AnchorId.Value,
                Stage = Stage1SettlementProgressStage.DamagedCampStart,
                Objective = Stage1FlowObjective.RepairCamp,
                Hint = Stage1FlowHint.GatherRepairResources
            });
            entity.Set(new Stage1ProgressionState(spec.AnchorId, spec.StartingFlagsMask));
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
            entity.Set(new BossBuildPreparationState
            {
                Status = BossBuildPreparationStatus.None
            });
            entity.Set(new BossPreparedBuildSnapshot());
        }
    }
}
