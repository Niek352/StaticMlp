using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class ClientStage1PresentationBootstrapSystem : ISystem
    {
        public void Init()
        {
            CW.SetResource(new Stage1HudSession
            {
                IsVisible = true,
            });
            CW.SetResource(new InteractionPromptState());
            CW.SetResource(new BuildingPanelSession());
        }
    }
}
