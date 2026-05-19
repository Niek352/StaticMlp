using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Frontier
{
    public struct ExpeditionSelectionScreenState : IResource
    {
        public SettlementAnchorId AnchorId;
        public ExpeditionId ExpeditionId;
        public BossId BossId;
        public RewardPackageId RewardPackageId;
        public LoadoutModuleId PreparedPrimaryModuleId;
        public ExpeditionAvailabilityStatus AvailabilityStatus;
        public ExpeditionActivityStatus ActivityStatus;
        public ThreatPhase ThreatPhase;
        public BossEncounterStatus BossStatus;
        public bool IsBossEncounterMode;
        public bool CanStart;
    }
}
