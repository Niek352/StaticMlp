using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Progression
{
    public sealed class ClientProgressionPresentationBootstrapSystem : ISystem
    {
        public void Init()
        {
            CW.SetResource(new RewardResultPopupSession());
            CW.SetResource(new ProgressionHudState());
            CW.NewEntity<Default>().Set(new RewardResultPopupViewData());
        }
    }
}
