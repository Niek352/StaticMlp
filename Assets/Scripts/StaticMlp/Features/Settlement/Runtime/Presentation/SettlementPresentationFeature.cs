using Code.EcsUi.Mvc;
using StaticMlp.Features.Frontier;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementPresentationFeature : GameplayFeature
    {
        private const string HUD_VIEW_RESOURCE_PATH = "Views/Stage1/Stage1HudView";
        private const string CONTEXT_PANEL_VIEW_RESOURCE_PATH = "Views/Stage1/Stage1ContextPanelView";
        private const string THREAT_BANNER_VIEW_RESOURCE_PATH = "Views/Stage1/ThreatBannerView";

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            var hudBridge = new ControllerResourceBridgeSystem<Stage1HudController, Stage1HudState>((controller, state) => controller.Apply(in state));
            var contextBridge = new ControllerResourceBridgeSystem<Stage1ContextPanelController, Stage1ContextPanelState>((controller, state) => controller.Apply(in state));
            var threatBridge = new ControllerResourceBridgeSystem<ThreatBannerController, ThreatBannerState>((controller, state) => controller.Apply(in state));

            systems.Add(new ClientStage1PresentationBootstrapSystem(), GameplaySystemOrder.ClientPresentation + 1);
            systems.Add(new ClientStage1HudStateSystem(), GameplaySystemOrder.ClientPresentation + 2);
            systems.Add(new ClientStage1ContextPanelSessionSystem(), GameplaySystemOrder.ClientPresentation + 3);
            systems.Add(new ClientStage1ContextPanelStateSystem(), GameplaySystemOrder.ClientPresentation + 4);
            systems.Add(new StateDrivenPersistentControllerHostSystem<Stage1HudView, Stage1HudController, Stage1HudSession>(
                manager => new Stage1HudController(manager, ResourcesViewFactory.CreateLazy<Stage1HudView>(HUD_VIEW_RESOURCE_PATH), hudBridge),
                state => state.IsVisible,
                (controller, _) =>
                {
                    var state = CW.GetResource<Stage1HudState>();
                    controller.Apply(in state);
                }), GameplaySystemOrder.ClientPresentation + 8);
            systems.Add(new PersistentControllerHostSystem<Stage1ContextPanelView, Stage1ContextPanelController>(
                _ => new Stage1ContextPanelController(ResourcesViewFactory.CreateLazy<Stage1ContextPanelView>(CONTEXT_PANEL_VIEW_RESOURCE_PATH), contextBridge),
                controller =>
                {
                    var state = CW.GetResource<Stage1ContextPanelState>();
                    controller.Apply(in state);
                }), GameplaySystemOrder.ClientPresentation + 9);
            systems.Add(new PersistentControllerHostSystem<ThreatBannerView, ThreatBannerController>(
                _ => new ThreatBannerController(ResourcesViewFactory.CreateLazy<ThreatBannerView>(THREAT_BANNER_VIEW_RESOURCE_PATH), threatBridge),
                controller =>
                {
                    var state = CW.GetResource<ThreatBannerState>();
                    controller.Apply(in state);
                }), GameplaySystemOrder.ClientPresentation + 10);
        }
    }
}
