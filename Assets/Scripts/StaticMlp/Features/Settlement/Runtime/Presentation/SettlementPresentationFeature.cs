using Code.EcsUi.Mvc;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementPresentationFeature : GameplayFeature
    {
        private const string HUD_VIEW_RESOURCE_PATH = "Views/Stage1/Stage1HudView";
        private const string INTERACTION_PROMPT_VIEW_RESOURCE_PATH = "Views/Settlement/InteractionPromptView";
        private const string BUILDING_MANAGEMENT_PANEL_VIEW_RESOURCE_PATH = "Views/Settlement/BuildingManagementPanelView";

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            var hudBridge = new Stage1HudCompositeBridgeSystem();
            var promptBridge = new InteractionPromptBridgeSystem();
            var buildingPanelBridge = new BuildingManagementPanelBridgeSystem();

            systems.Add(new ClientStage1PresentationBootstrapSystem(), GameplaySystemOrder.ClientPresentation + 1);
            systems.Add(new ClientBuildingManagementPanelInteractionOpenSystem(), GameplaySystemOrder.ClientPresentation + 2);
            systems.Add(new ClientBuildingManagementPanelCloseInputSystem(), GameplaySystemOrder.ClientPresentation + 3);
            systems.Add(new ClientBuildingManagementPanelLifetimeSystem(), GameplaySystemOrder.ClientPresentation + 4);
            systems.Add(new ClientSettlementInteractionPromptStateSystem(), GameplaySystemOrder.ClientPresentation + 5);
            systems.Add(new StateDrivenPersistentControllerHostSystem<Stage1HudView, Stage1HudController, Stage1HudSession>(
                manager => new Stage1HudController(manager, ResourcesViewFactory.CreateLazy<Stage1HudView>(HUD_VIEW_RESOURCE_PATH), hudBridge),
                state => state.IsVisible), GameplaySystemOrder.ClientPresentation + 8);
            systems.Add(hudBridge, GameplaySystemOrder.ClientPresentation + 9);
            systems.Add(new StateDrivenPersistentControllerHostSystem<InteractionPromptView, InteractionPromptController, InteractionPromptState>(
                _ => new InteractionPromptController(
                    ResourcesViewFactory.CreateLazy<InteractionPromptView>(INTERACTION_PROMPT_VIEW_RESOURCE_PATH),
                    promptBridge),
                state => state.IsVisible), GameplaySystemOrder.ClientPresentation + 10);
            systems.Add(promptBridge, GameplaySystemOrder.ClientPresentation + 11);
            systems.Add(new StateDrivenPersistentControllerHostSystem<BuildingManagementPanelView, BuildingManagementPanelController, BuildingManagementPanelSession>(
                _ => new BuildingManagementPanelController(
                    ResourcesViewFactory.CreateLazy<BuildingManagementPanelView>(BUILDING_MANAGEMENT_PANEL_VIEW_RESOURCE_PATH),
                    buildingPanelBridge),
                state => state.IsOpen), GameplaySystemOrder.ClientPresentation + 12);
            systems.Add(buildingPanelBridge, GameplaySystemOrder.ClientPresentation + 13);
        }
    }
}
