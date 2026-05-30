using Aspid.StaticEcs.Windows;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Progression;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementHudCompositeBridgeSystem
        : EcsWindowPresentationBridgeSystem<ClientCoreWT, SettlementHudWindow, SettlementHudSlot, SettlementHudViewModel>
    {
        protected override void SyncPresentation(SettlementHudViewModel viewModel)
        {
            var settlement = SettlementHudPresentation.Build();
            var expedition = ExpeditionHudPresentation.Build();
            var loadout = LoadoutHudPresentation.Build();
            var threat = ThreatHudPresentation.Build();
            var raid = RaidHudPresentation.Build();
            var boss = BossHudPresentation.Build();
            var progression = ProgressionHudPresentation.Build();

            viewModel.Sync(in settlement, in expedition, in loadout, in threat, in raid, in boss, in progression);
        }
    }
}
