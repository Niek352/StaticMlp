using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Features.Settlement.Workers;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.Stage1
{
    [NetworkEntityManifest(
        typeof(Stage1SettlementProgression),
        typeof(Stage1FlowViewState),
        typeof(Stage1ProgressionState),
        typeof(SettlementWorkerSummary),
        typeof(SettlementCampBuilderJobState),
        typeof(ExpeditionAvailabilityState),
        typeof(ActiveExpeditionState),
        typeof(ThreatState),
        typeof(RaidScheduleState),
        typeof(BossEncounterState),
        typeof(BossLoadoutPreparationState),
        typeof(BossPreparedLoadoutSnapshot))]
    public struct Stage1CampAnchorNetworkEntity : INetworkEntityType
    {
        public byte Id() => 8;
        public ushort NetworkSchemaVersion() => 1;
        public ushort DefaultNetworkArchetypeId() => SettlementNetworkArchetypeIds.CampAnchor;
    }
}
