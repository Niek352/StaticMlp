using Code.EcsUi.Mvc;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Progression;

namespace StaticMlp.Features.Settlement
{
    public sealed class Stage1HudCompositeBridgeSystem : ControllerEcsBridgeSystem<Stage1HudController>
    {
        protected override void SyncPresentation()
        {
            var settlement = SettlementHudPresentation.Build();
            var expedition = ExpeditionHudPresentation.Build();
            var loadout = LoadoutHudPresentation.Build();
            var threat = ThreatHudPresentation.Build();
            var raid = RaidHudPresentation.Build();
            var boss = BossHudPresentation.Build();
            var progression = ProgressionHudPresentation.Build();

            Controller.Apply(in settlement, in expedition, in loadout, in threat, in raid, in boss, in progression);
        }
    }
}
