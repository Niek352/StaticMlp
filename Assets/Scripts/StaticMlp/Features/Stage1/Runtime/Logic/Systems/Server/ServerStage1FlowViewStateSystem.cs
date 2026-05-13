using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Stage1
{
    public sealed class ServerStage1FlowViewStateSystem : ISystem
    {
        public void Update()
        {
            foreach (var anchor in SW.Query<All<Stage1SettlementProgression, Stage1ProgressionState, ExpeditionAvailabilityState, ActiveExpeditionState, ThreatState, RaidScheduleState, BossEncounterState, Stage1FlowViewState>>().Entities())
            {
                var next = BuildState(anchor);
                if (anchor.Read<Stage1FlowViewState>().Equals(next))
                    continue;

                ref var mutable = ref ReplicationMut.Mut<Stage1FlowViewState>(anchor);
                mutable = next;
            }
        }

        private static Stage1FlowViewState BuildState(SW.Entity anchor)
        {
            ref readonly var progression = ref anchor.Read<Stage1SettlementProgression>();
            ref readonly var progressionState = ref anchor.Read<Stage1ProgressionState>();
            ref readonly var availability = ref anchor.Read<ExpeditionAvailabilityState>();
            ref readonly var expedition = ref anchor.Read<ActiveExpeditionState>();
            ref readonly var threat = ref anchor.Read<ThreatState>();
            ref readonly var raid = ref anchor.Read<RaidScheduleState>();
            ref readonly var boss = ref anchor.Read<BossEncounterState>();

            return new Stage1FlowViewState
            {
                AnchorId = progression.AnchorId,
                Stage = progression.Stage,
                Objective = ResolveObjective(progression.Stage, in progressionState, in availability, in expedition, in threat, in boss),
                Hint = ResolveHint(progression.Stage),
                CanToggleWorkerAssignment = progression.Stage >= Stage1SettlementProgressStage.CampRepaired,
                CanOpenBuildPreparation =
                    progression.Stage >= Stage1SettlementProgressStage.WorkerAssigned
                    && boss.Status != BossEncounterStatus.Active
                    && boss.Status != BossEncounterStatus.Defeated,
                CanOpenExpeditionSelection =
                    (availability.Status == ExpeditionAvailabilityStatus.Available
                     || boss.Status == BossEncounterStatus.Available)
                    && expedition.Status == ExpeditionActivityStatus.None
                    && threat.Phase != ThreatPhase.RaidPending
                    && threat.Phase != ThreatPhase.RaidActive
                    && raid.Status == RaidScheduleStatus.None
            };
        }

        private static Stage1FlowObjective ResolveObjective(
            Stage1SettlementProgressStage stage,
            in Stage1ProgressionState progression,
            in ExpeditionAvailabilityState availability,
            in ActiveExpeditionState expedition,
            in ThreatState threat,
            in BossEncounterState boss)
        {
            if (boss.Status == BossEncounterStatus.Defeated)
                return Stage1FlowObjective.VerticalSliceComplete;

            if (boss.Status == BossEncounterStatus.Active)
                return Stage1FlowObjective.DefeatBoss;

            if (boss.Status == BossEncounterStatus.Available)
                return Stage1FlowObjective.StartBossEncounter;

            if (threat.Phase is ThreatPhase.RaidPending or ThreatPhase.RaidActive)
                return Stage1FlowObjective.DefendCamp;

            if (expedition.Status == ExpeditionActivityStatus.Active)
                return Stage1FlowObjective.ClearExpedition;

            if (stage < Stage1SettlementProgressStage.CampRepaired)
                return Stage1FlowObjective.RepairCamp;

            if (stage < Stage1SettlementProgressStage.WorkerAssigned)
                return Stage1FlowObjective.AssignWorker;

            if (stage < Stage1SettlementProgressStage.BuildPrepared)
                return Stage1FlowObjective.PrepareBuild;

            if (availability.Status == ExpeditionAvailabilityStatus.Available)
                return Stage1FlowObjective.StartExpedition;

            if (progression.HasFlag(ProgressFlagCatalog.CounterattackDefendedId)
                && !progression.HasFlag(ProgressFlagCatalog.BossUnlockedId))
            {
                return Stage1FlowObjective.PrepareBoss;
            }

            if (progression.HasFlag(ProgressFlagCatalog.BossUnlockedId))
                return Stage1FlowObjective.StartBossEncounter;

            return Stage1FlowObjective.PrepareBuild;
        }

        private static Stage1FlowHint ResolveHint(Stage1SettlementProgressStage stage)
        {
            if (stage == Stage1SettlementProgressStage.RepairResourcesReady)
                return Stage1FlowHint.ContinueRepairBuild;

            if (stage < Stage1SettlementProgressStage.RepairResourcesReady)
                return Stage1FlowHint.GatherRepairResources;

            if (stage == Stage1SettlementProgressStage.CampRepaired)
                return Stage1FlowHint.AssignWorker;

            return Stage1FlowHint.None;
        }
    }
}
