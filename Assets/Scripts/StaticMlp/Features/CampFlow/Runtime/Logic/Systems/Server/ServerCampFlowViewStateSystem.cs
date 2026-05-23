using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.CampFlow
{
    public sealed class ServerCampFlowViewStateSystem : ISystem
    {
        private static readonly IFlowObjectiveOverride[] OBJECTIVE_OVERRIDES =
        {
            new BossFlowObjectiveOverride(),
            new ThreatFlowObjectiveOverride(),
            new ExpeditionFlowObjectiveOverride(),
            new ProgressionFlowObjectiveOverride(),
        };

        public void Update()
        {
            foreach (var anchor in SW.Query<All<CampFlowProgression, ProgressionState, ExpeditionAvailabilityState, ActiveExpeditionState, ThreatState, RaidScheduleState, BossEncounterState, CampFlowViewState>>().Entities())
            {
                var next = BuildState(anchor);
                if (anchor.Read<CampFlowViewState>().Equals(next))
                    continue;

                ref var mutable = ref ReplicationMut.Mut<CampFlowViewState>(anchor);
                mutable = next;
            }
        }

        private static CampFlowViewState BuildState(SW.Entity anchor)
        {
            ref readonly var progression = ref anchor.Read<CampFlowProgression>();
            ref readonly var progressionState = ref anchor.Read<ProgressionState>();
            ref readonly var availability = ref anchor.Read<ExpeditionAvailabilityState>();
            ref readonly var expedition = ref anchor.Read<ActiveExpeditionState>();
            ref readonly var threat = ref anchor.Read<ThreatState>();
            ref readonly var raid = ref anchor.Read<RaidScheduleState>();
            ref readonly var boss = ref anchor.Read<BossEncounterState>();
            var context = new CampFlowContext(
                progression.Stage,
                in progressionState,
                in availability,
                in expedition,
                in threat,
                in boss);
            var definition = ResolveDefinition(in context);

            return new CampFlowViewState
            {
                AnchorId = progression.AnchorId,
                Stage = progression.Stage,
                ObjectiveDisplayName = definition.ObjectiveDisplayName,
                HintDisplayName = definition.HintDisplayName,
                CanToggleWorkerAssignment = progression.Stage >= CampFlowStage.CampRepaired,
                CanOpenLoadoutPreparation =
                    progression.Stage >= CampFlowStage.WorkbenchOnline
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

        private static CampFlowStageDefinition ResolveDefinition(in CampFlowContext context)
        {
            for (var i = 0; i < OBJECTIVE_OVERRIDES.Length; i++)
            {
                if (OBJECTIVE_OVERRIDES[i].TryOverride(
                        in context,
                        out var objectiveDisplayName,
                        out var hintDisplayName))
                {
                    return new CampFlowStageDefinition(
                        context.Stage,
                        objectiveDisplayName,
                        hintDisplayName,
                        autoAdvance: false);
                }
            }

            return CampFlowCatalog.Get(context.Stage);
        }
    }
}
