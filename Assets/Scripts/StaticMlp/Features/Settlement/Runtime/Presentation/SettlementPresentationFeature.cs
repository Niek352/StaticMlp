using Code.EcsUi.Mvc;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementPresentationFeature : GameplayFeature
    {
        private const string HUD_VIEW_RESOURCE_PATH = "Views/Stage1/Stage1HudView";
        private const string CONTEXT_PANEL_VIEW_RESOURCE_PATH = "Views/Stage1/Stage1ContextPanelView";

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            var hudBridge = new Stage1HudCompositeBridgeSystem();
            var contextBridge = new Stage1ContextPanelCompositeBridgeSystem();

            systems.Add(new ClientStage1PresentationBootstrapSystem(), GameplaySystemOrder.ClientPresentation + 1);
            systems.Add(new ClientStage1ContextPanelSessionSystem(), GameplaySystemOrder.ClientPresentation + 3);
            systems.Add(new StateDrivenPersistentControllerHostSystem<Stage1HudView, Stage1HudController, Stage1HudSession>(
                manager => new Stage1HudController(manager, ResourcesViewFactory.CreateLazy<Stage1HudView>(HUD_VIEW_RESOURCE_PATH), hudBridge),
                state => state.IsVisible), GameplaySystemOrder.ClientPresentation + 8);
            systems.Add(hudBridge, GameplaySystemOrder.ClientPresentation + 9);
            systems.Add(new PersistentControllerHostSystem<Stage1ContextPanelView, Stage1ContextPanelController>(
                _ => new Stage1ContextPanelController(ResourcesViewFactory.CreateLazy<Stage1ContextPanelView>(CONTEXT_PANEL_VIEW_RESOURCE_PATH), contextBridge)), GameplaySystemOrder.ClientPresentation + 10);
            systems.Add(contextBridge, GameplaySystemOrder.ClientPresentation + 11);
        }
    }
}
