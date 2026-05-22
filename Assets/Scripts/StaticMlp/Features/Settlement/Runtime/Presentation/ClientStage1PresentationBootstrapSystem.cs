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
            CW.SetResource(new Stage1ContextPanelSession
            {
                Mode = Stage1ContextPanelMode.Worker,
                WorkerAnchorId = SettlementAnchorCatalog.HomeCampId,
            });
            CW.SetResource(new Stage1ContextFocusTarget());
            CW.SetResource(new Stage1BuildingOperationOpenIntent());
        }
    }
}
