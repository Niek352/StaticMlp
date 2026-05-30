using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Frontier;
using StaticMlp.Features.Loadout;
using StaticMlp.Features.Progression;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementHudCompositeBridgeSystem : ISystem
    {
        public void Update()
        {
            var viewData = new SettlementHudViewData
            {
                Settlement = SettlementHudPresentation.Build(),
                Expedition = ExpeditionHudPresentation.Build(),
                Loadout = LoadoutHudPresentation.Build(),
                Threat = ThreatHudPresentation.Build(),
                Raid = RaidHudPresentation.Build(),
                Boss = BossHudPresentation.Build(),
                Progression = ProgressionHudPresentation.Build()
            };

            foreach (var entity in CW.Query<All<SettlementHudViewData>>().Entities())
            {
                ref var data = ref entity.Mut<SettlementHudViewData>();
                data = viewData;
                return;
            }

            throw new InvalidOperationException($"{nameof(SettlementHudViewData)} entity is missing.");
        }
    }
}
