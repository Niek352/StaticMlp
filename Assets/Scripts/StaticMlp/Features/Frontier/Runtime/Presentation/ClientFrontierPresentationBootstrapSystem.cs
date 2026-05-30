using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Networking;

namespace StaticMlp.Features.Frontier
{
    public sealed class ClientFrontierPresentationBootstrapSystem : ISystem
    {
        public void Init()
        {
            CW.NewEntity<Default>().Set(new ExpeditionSelectionViewData());
            CW.NewEntity<Default>().Set(new ThreatBannerViewData());
        }
    }
}
