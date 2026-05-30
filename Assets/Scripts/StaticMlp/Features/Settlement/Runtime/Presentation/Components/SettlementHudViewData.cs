using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Progression;

namespace StaticMlp.Features.Settlement
{
    public struct SettlementHudViewData : IComponent, ITrackableAdded, ITrackableChanged
    {
        public SettlementHudState Settlement;
        public ExpeditionHudState Expedition;
        public LoadoutHudState Loadout;
        public ThreatHudState Threat;
        public RaidHudState Raid;
        public BossHudState Boss;
        public ProgressionHudState Progression;
    }
}
