using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Progression;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Frontier
{
    public sealed class ClientFrontierPresentationBootstrapSystem : ISystem
    {
        public void Init()
        {
            CW.SetResource(new ThreatBannerState
            {
                IsVisible = true,
                Phase = ThreatPhase.Calm,
            });
            CW.SetResource(new ExpeditionSelectionScreenState
            {
                AnchorId = SettlementAnchorCatalog.HomeCampId,
                ExpeditionId = ExpeditionCatalog.NearbyRaiderCampId,
                RewardPackageId = RewardPackageCatalog.RecoveredWarCacheId,
            });
            CW.SetResource(new ExpeditionHudState());
            CW.SetResource(new ThreatHudState());
            CW.SetResource(new RaidHudState());
            CW.SetResource(new BossHudState());
        }
    }
}
