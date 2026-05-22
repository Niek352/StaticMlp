using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Progression;

namespace StaticMlp.Features.Settlement
{
    public sealed class Stage1HudCompositeBridgeSystem : ControllerEcsBridgeSystem<Stage1HudController>
    {
        protected override void SyncPresentation()
        {
            var settlement = CW.GetResource<SettlementHudState>();
            var expedition = CW.GetResource<ExpeditionHudState>();
            var loadout = CW.GetResource<LoadoutHudState>();
            var threat = CW.GetResource<ThreatHudState>();
            var raid = CW.GetResource<RaidHudState>();
            var boss = CW.GetResource<BossHudState>();
            var progression = CW.GetResource<ProgressionHudState>();

            Controller.Apply(in settlement, in expedition, in loadout, in threat, in raid, in boss, in progression);
        }
    }
}
