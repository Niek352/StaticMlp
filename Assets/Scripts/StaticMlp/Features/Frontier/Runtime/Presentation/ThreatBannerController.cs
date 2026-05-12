using Code.EcsUi.Mvc;

namespace StaticMlp.Features.Frontier
{
    public sealed class ThreatBannerController
        : ControllerBase<ThreatBannerView>, IResourcePresentationController<ThreatBannerState>
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
