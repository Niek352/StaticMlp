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
            if (!TryGetRequiredState(out var anchor, out var flow, out var resources))
                return;

            var next = new Stage1HudState
            {
                AnchorId = flow.Anchor,
                Objective = ToPresentationObjective(flow.Objective),
                ObjectiveHint = ToPresentationHint(flow.Hint),
                SettlementStage = flow.Stage,
                Wood = resources.Wood,
                Stone = resources.Stone,
                CanOpenBuildPreparation = flow.CanOpenBuildPreparation,
                CanOpenExpeditionSelection = flow.CanOpenExpeditionSelection,
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

            CW.SetResource(next);
        }

        private static bool TryGetRequiredState(
            out CW.Entity anchor,
            out Stage1FlowViewState flow,
            out SettlementSharedResources resources)
        {
            if (!Stage1SettlementProgressionQuery.TryGetClientAnchor(SettlementAnchorCatalog.HomeCampId, out anchor))
            {
                flow = default;
                resources = default;
                return false;
            }

            flow = ClientProjection.Read<Stage1FlowViewState>(anchor);

            if (!TryReadSharedResources(out resources))
                return false;

            return true;
        }

        private static Stage1ObjectiveKind ToPresentationObjective(Stage1FlowObjective objective)
        {
            switch (objective)
            {
                case Stage1FlowObjective.RepairCamp:
                    return Stage1ObjectiveKind.RepairCamp;
                case Stage1FlowObjective.AssignWorker:
                    return Stage1ObjectiveKind.AssignWorker;
                case Stage1FlowObjective.PrepareBuild:
                    return Stage1ObjectiveKind.PrepareBuild;
                case Stage1FlowObjective.StartExpedition:
                    return Stage1ObjectiveKind.StartExpedition;
                case Stage1FlowObjective.ClearExpedition:
                    return Stage1ObjectiveKind.ClearExpedition;
                case Stage1FlowObjective.DefendCamp:
                    return Stage1ObjectiveKind.DefendCamp;
                case Stage1FlowObjective.PrepareBoss:
                    return Stage1ObjectiveKind.PrepareBoss;
                case Stage1FlowObjective.StartBossEncounter:
                    return Stage1ObjectiveKind.StartBossEncounter;
                case Stage1FlowObjective.DefeatBoss:
                    return Stage1ObjectiveKind.DefeatBoss;
                case Stage1FlowObjective.VerticalSliceComplete:
                    return Stage1ObjectiveKind.VerticalSliceComplete;
                default:
                    return Stage1ObjectiveKind.None;
            }
        }

        private static string ToPresentationHint(Stage1FlowHint hint)
        {
            switch (hint)
            {
                case Stage1FlowHint.GatherRepairResources:
                    return "Gather the camp resources needed to begin repairs.";
                case Stage1FlowHint.ContinueRepairBuild:
                    return "Resources delivered. Keep building the camp core to finish repairs.";
                case Stage1FlowHint.AssignWorker:
                    return "The camp is repaired. Assign the camp builder to continue the loop.";
                default:
                    return string.Empty;
            }
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
