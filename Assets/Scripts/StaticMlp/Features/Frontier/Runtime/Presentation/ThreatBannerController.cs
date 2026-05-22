using System;
using Code.EcsUi.Mvc;

namespace StaticMlp.Features.Frontier
{
    public sealed class ThreatBannerController
        : ControllerBase<ThreatBannerView>
    {
        public ThreatBannerController(
            ViewFactoryMethod<ThreatBannerView> viewFactory,
            ThreatBannerBridgeSystem bridge)
            : base(viewFactory)
        {
            AddModule(new BridgeSystemBinding<ThreatBannerBridgeSystem, ThreatBannerController>(this, bridge));
        }

        public override ViewLayer Layer => ViewLayer.Persistent;
        public override int? PersistentSortOrder => 200;

        public void Apply(in ThreatBannerState state)
        {
            View.Render(in state);
        }
    }
}
