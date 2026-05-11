using Code.EcsUi.Mvc;
using StaticMlp.Features.Frontier;

namespace StaticMlp.Features.Settlement
{
    public sealed class ThreatBannerController : ControllerBase<ThreatBannerView>
    {
        public ThreatBannerController(
            ViewFactoryMethod<ThreatBannerView> viewFactory,
            ControllerResourceBridgeSystem<ThreatBannerController, ThreatBannerState> bridge)
            : base(viewFactory)
        {
            AddModule(new BridgeSystemBinding<ControllerResourceBridgeSystem<ThreatBannerController, ThreatBannerState>, ThreatBannerController>(this, bridge));
        }

        public override ViewLayer Layer => ViewLayer.Persistent;
        public override int? PersistentSortOrder => 200;

        public void Apply(in ThreatBannerState state)
        {
            View.Render(in state);
        }
    }
}
