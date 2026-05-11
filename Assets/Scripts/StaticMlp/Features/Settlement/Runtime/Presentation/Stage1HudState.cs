using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Build;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement.Workers;

namespace StaticMlp.Features.Settlement
{
    public struct Stage1HudState : IResource
    {
        public SettlementAnchorId AnchorId;
        public Stage1ObjectiveKind Objective;
        public string ObjectiveHint;
        public Stage1SettlementProgressStage SettlementStage;
        public int Wood;
        public int Stone;
        public ushort TotalWorkers;
        public ushort AssignedWorkers;
        public SettlementWorkerBlockingReason WorkerBlockingReason;
        public BuildModuleId PreparedPrimaryModuleId;
        public bool HasPreparedBuild;
        public ExpeditionAvailabilityStatus ExpeditionAvailability;
        public ExpeditionActivityStatus ExpeditionActivity;
        public ThreatPhase ThreatPhase;
        public RaidScheduleStatus RaidScheduleStatus;
        public uint RaidActivateAtTick;
        public bool HasRecoveredWarCache;
        public bool HasCounterattackDefended;
        public bool HasBossUnlocked;
        public byte BossPreparationTokens;
        public BossEncounterStatus BossEncounterStatus;
        public bool CanOpenBuildPreparation;
        public bool CanOpenExpeditionSelection;
    }
}
