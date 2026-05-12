using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Progression
{
    public sealed class ClientProgressionPresentationBootstrapSystem : ISystem
    {
        public void Init()
        {
            CW.SetResource(new RewardResultPopupState());
        }
    }
}
