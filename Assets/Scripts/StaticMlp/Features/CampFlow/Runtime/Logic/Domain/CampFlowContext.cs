using StaticMlp.Features.Frontier;
using StaticMlp.Features.Progression;

namespace StaticMlp.Features.CampFlow
{
    public readonly struct CampFlowContext
    {
        public readonly CampFlowStage Stage;
        public readonly ProgressionState Progression;
        public readonly ExpeditionAvailabilityState Availability;
        public readonly ActiveExpeditionState Expedition;
        public readonly ThreatState Threat;
        public readonly BossEncounterState Boss;

        public CampFlowContext(
            CampFlowStage stage,
            in ProgressionState progression,
            in ExpeditionAvailabilityState availability,
            in ActiveExpeditionState expedition,
            in ThreatState threat,
            in BossEncounterState boss)
        {
            Stage = stage;
            Progression = progression;
            Availability = availability;
            Expedition = expedition;
            Threat = threat;
            Boss = boss;
        }
    }
}
