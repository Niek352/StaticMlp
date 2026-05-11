using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Build;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;

namespace StaticMlp.Features.Settlement
{
    public sealed class ClientStage1HudStateSystem : ISystem
    {
        public void Update()
        {
            if (!TryGetRequiredState(out var anchor, out var settlementProgression, out var resources))
                return;

            var next = new Stage1HudState
            {
                AnchorId = settlementProgression.Anchor,
                Objective = Stage1ObjectiveKind.RepairCamp,
                SettlementStage = settlementProgression.Stage,
                Wood = resources.Wood,
                Stone = resources.Stone,
            };

            if (anchor.Has<Projected<Stage1ProgressionState>>())
            {
                ref readonly var progression = ref ClientProjection.Read<Stage1ProgressionState>(anchor);
                next.HasRecoveredWarCache = progression.HasFlag(ProgressFlagCatalog.RecoveredWarCacheAppliedId);
                next.HasCounterattackDefended = progression.HasFlag(ProgressFlagCatalog.CounterattackDefendedId);
                next.HasBossUnlocked = progression.HasFlag(ProgressFlagCatalog.BossUnlockedId);
                next.BossPreparationTokens = progression.BossPreparationTokens;
            }

            if (anchor.Has<Projected<SettlementWorkerSummary>>())
            {
                ref readonly var summary = ref ClientProjection.Read<SettlementWorkerSummary>(anchor);
                next.TotalWorkers = summary.TotalWorkers;
                next.AssignedWorkers = summary.AssignedWorkers;
                next.WorkerBlockingReason = summary.BlockingReason;
            }

            if (anchor.Has<Projected<ExpeditionAvailabilityState>>())
                next.ExpeditionAvailability = ClientProjection.Read<ExpeditionAvailabilityState>(anchor).Status;

            if (anchor.Has<Projected<ActiveExpeditionState>>())
                next.ExpeditionActivity = ClientProjection.Read<ActiveExpeditionState>(anchor).Status;

            if (anchor.Has<Projected<ThreatState>>())
                next.ThreatPhase = ClientProjection.Read<ThreatState>(anchor).Phase;

            if (anchor.Has<Projected<RaidScheduleState>>())
            {
                ref readonly var raid = ref ClientProjection.Read<RaidScheduleState>(anchor);
                next.RaidScheduleStatus = raid.Status;
                next.RaidActivateAtTick = raid.ActivateAtTick;
            }

            if (anchor.Has<Projected<BossEncounterState>>())
                next.BossEncounterStatus = ClientProjection.Read<BossEncounterState>(anchor).Status;

            if (TryReadPreparedBuild(out var preparedBuild))
            {
                next.PreparedPrimaryModuleId = preparedBuild.PrimaryModuleId;
                next.HasPreparedBuild = preparedBuild.PrimaryModuleId.Value != 0;
            }

            next.CanOpenBuildPreparation = next.SettlementStage >= Stage1SettlementProgressStage.CampRepaired
                                           && next.BossEncounterStatus != BossEncounterStatus.Active
                                           && next.BossEncounterStatus != BossEncounterStatus.Defeated;
            next.CanOpenExpeditionSelection = next.ExpeditionAvailability == ExpeditionAvailabilityStatus.Available
                                              && next.ThreatPhase != ThreatPhase.RaidPending
                                              && next.ThreatPhase != ThreatPhase.RaidActive;
            next.Objective = ResolveObjective(next);
            next.ObjectiveHint = ResolveObjectiveHint(next);

            CW.SetResource(next);
        }

        private static bool TryGetRequiredState(
            out CW.Entity anchor,
            out Stage1SettlementProgression settlementProgression,
            out SettlementSharedResources resources)
        {
            if (!Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out anchor))
            {
                settlementProgression = default;
                resources = default;
                return false;
            }

            settlementProgression = anchor.Read<Stage1SettlementProgression>();

            if (!TryReadSharedResources(out resources))
                return false;

            return true;
        }

        private static Stage1ObjectiveKind ResolveObjective(in Stage1HudState state)
        {
            if (state.BossEncounterStatus == BossEncounterStatus.Defeated)
                return Stage1ObjectiveKind.VerticalSliceComplete;

            if (state.BossEncounterStatus == BossEncounterStatus.Active)
                return Stage1ObjectiveKind.DefeatBoss;

            if (state.BossEncounterStatus == BossEncounterStatus.Available)
                return Stage1ObjectiveKind.StartBossEncounter;

            if (state.ThreatPhase == ThreatPhase.RaidPending || state.ThreatPhase == ThreatPhase.RaidActive)
                return Stage1ObjectiveKind.DefendCamp;

            if (state.ExpeditionActivity == ExpeditionActivityStatus.Active)
                return Stage1ObjectiveKind.ClearExpedition;

            if (state.SettlementStage < Stage1SettlementProgressStage.CampRepaired)
                return Stage1ObjectiveKind.RepairCamp;

            if (state.SettlementStage < Stage1SettlementProgressStage.WorkerAssigned)
                return Stage1ObjectiveKind.AssignWorker;

            if (state.SettlementStage < Stage1SettlementProgressStage.BuildPrepared)
                return Stage1ObjectiveKind.PrepareBuild;

            if (state.ExpeditionAvailability == ExpeditionAvailabilityStatus.Available)
                return Stage1ObjectiveKind.StartExpedition;

            if (state.HasCounterattackDefended && !state.HasBossUnlocked)
                return Stage1ObjectiveKind.PrepareBoss;

            if (state.HasBossUnlocked)
                return Stage1ObjectiveKind.StartBossEncounter;

            return Stage1ObjectiveKind.PrepareBuild;
        }

        private static string ResolveObjectiveHint(in Stage1HudState state)
        {
            if (state.SettlementStage == Stage1SettlementProgressStage.RepairResourcesReady)
                return "Resources delivered. Keep building the camp core to finish repairs.";

            if (state.SettlementStage < Stage1SettlementProgressStage.RepairResourcesReady)
                return "Gather the camp resources needed to begin repairs.";

            if (state.Objective == Stage1ObjectiveKind.AssignWorker)
                return "The camp is repaired. Assign the camp builder to continue the loop.";

            return string.Empty;
        }

        private static bool TryReadPreparedBuild(out PreparedBuildSnapshot snapshot)
        {
            foreach (var player in CW.Query<All<PreparedBuildSnapshot>>().Entities())
            {
                snapshot = player.Read<PreparedBuildSnapshot>();
                return true;
            }

            snapshot = default;
            return false;
        }

        private static bool TryReadSharedResources(out SettlementSharedResources resources)
        {
            foreach (var entity in CW.Query<All<SettlementResourceStorageTag, SettlementSharedResources>>().Entities())
            {
                resources = entity.Read<SettlementSharedResources>();
                return true;
            }

            resources = default;
            return false;
        }
    }
}
