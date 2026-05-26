using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class ClientSettlementPresentationBootstrapSystem : ISystem
    {
        public void Init()
        {
            CW.SetResource(new SettlementHudSession
            {
                IsVisible = true,
            });
            CW.SetResource(new InteractionPromptState());
            CW.SetResource(new BuildingPanelSession());
            CW.SetResource(new SettlementContextPanelSession
            {
                Mode = SettlementContextPanelMode.None,
            });
        }
    }
}
