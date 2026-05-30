using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;

namespace StaticMlp.Features.Frontier
{
    public readonly struct ExpeditionSelectionStartIntent : IEvent
    {
        public readonly SettlementAnchorId AnchorId;
        public readonly ExpeditionId ExpeditionId;
        public readonly BossId BossId;
        public readonly bool IsBossEncounterMode;

        public ExpeditionSelectionStartIntent(
            SettlementAnchorId anchorId,
            ExpeditionId expeditionId,
            BossId bossId,
            bool isBossEncounterMode)
        {
            AnchorId = anchorId;
            ExpeditionId = expeditionId;
            BossId = bossId;
            IsBossEncounterMode = isBossEncounterMode;
        }
    }
}
