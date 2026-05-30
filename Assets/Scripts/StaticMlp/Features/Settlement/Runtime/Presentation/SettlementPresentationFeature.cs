using Aspid.StaticEcs.Windows;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Networking;

namespace StaticMlp.Features.Settlement
{
    public sealed class SettlementPresentationFeature : GameplayFeature
    {
        private const string HUD_VIEW_RESOURCE_PATH = "Views/Stage1/Stage1HudView";
        private const string INTERACTION_PROMPT_VIEW_RESOURCE_PATH = "Views/Settlement/InteractionPromptView";
        private const string BUILDING_MANAGEMENT_PANEL_VIEW_RESOURCE_PATH = "Views/Settlement/BuildingManagementPanelView";
        private const string CONTEXT_PANEL_VIEW_RESOURCE_PATH = "Views/Settlement/SettlementContextPanelView";

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            var hudBridge = new SettlementHudCompositeBridgeSystem();
            var promptBridge = new InteractionPromptBridgeSystem();
            var buildingPanelBridge = new BuildingManagementPanelBridgeSystem();
            var contextPanelBridge = new SettlementContextPanelCompositeBridgeSystem();
            var windows = CW.GetResource<WindowsController<ClientCoreWT>>();

            windows.RegisterWindow<SettlementHudWindow, EcsWindowNoData, SettlementHudView>(
                EcsResourcesWindowShellViewFactory.CreateLazy<SettlementHudView>(HUD_VIEW_RESOURCE_PATH),
                EcsWindowLayer.Persistent,
                persistentSortOrder: 0);
            windows.RegisterViewModel<SettlementHudWindow, EcsWindowNoData, SettlementHudSlot, SettlementHudViewModel>(
                static _ => new SettlementHudViewModel(),
                EcsWindowOpenInputBindings.Ignore<ClientCoreWT, SettlementHudWindow, EcsWindowNoData, SettlementHudViewModel>);

            windows.RegisterWindow<InteractionPromptWindow, EcsWindowNoData, InteractionPromptView>(
                EcsResourcesWindowShellViewFactory.CreateLazy<InteractionPromptView>(INTERACTION_PROMPT_VIEW_RESOURCE_PATH),
                EcsWindowLayer.Persistent,
                persistentSortOrder: 95);
            windows.RegisterViewModel<InteractionPromptWindow, EcsWindowNoData, InteractionPromptSlot, InteractionPromptViewModel>(
                static _ => new InteractionPromptViewModel(),
                EcsWindowOpenInputBindings.Ignore<ClientCoreWT, InteractionPromptWindow, EcsWindowNoData, InteractionPromptViewModel>);

            windows.RegisterWindow<BuildingManagementPanelWindow, EcsWindowNoData, BuildingManagementPanelView>(
                EcsResourcesWindowShellViewFactory.CreateLazy<BuildingManagementPanelView>(BUILDING_MANAGEMENT_PANEL_VIEW_RESOURCE_PATH),
                EcsWindowLayer.Persistent,
                persistentSortOrder: 110);
            windows.RegisterViewModel<BuildingManagementPanelWindow, EcsWindowNoData, BuildingManagementPanelSlot, BuildingManagementPanelViewModel>(
                static _ => new BuildingManagementPanelViewModel(),
                EcsWindowOpenInputBindings.Ignore<ClientCoreWT, BuildingManagementPanelWindow, EcsWindowNoData, BuildingManagementPanelViewModel>);

            windows.RegisterWindow<SettlementContextPanelWindow, EcsWindowNoData, SettlementContextPanelView>(
                EcsResourcesWindowShellViewFactory.CreateLazy<SettlementContextPanelView>(CONTEXT_PANEL_VIEW_RESOURCE_PATH),
                EcsWindowLayer.Persistent,
                persistentSortOrder: 100);
            windows.RegisterViewModel<SettlementContextPanelWindow, EcsWindowNoData, SettlementContextPanelSlot, SettlementContextPanelViewModel>(
                static _ => new SettlementContextPanelViewModel(),
                EcsWindowOpenInputBindings.Ignore<ClientCoreWT, SettlementContextPanelWindow, EcsWindowNoData, SettlementContextPanelViewModel>);

            systems.Add(new ClientSettlementPresentationBootstrapSystem(), GameplaySystemOrder.ClientPresentation + 1);
            systems.Add(new ClientBuildingManagementPanelInteractionOpenSystem(), GameplaySystemOrder.ClientPresentation + 2);
            systems.Add(new ClientBuildingManagementPanelCloseInputSystem(), GameplaySystemOrder.ClientPresentation + 3);
            systems.Add(new ClientBuildingManagementPanelLifetimeSystem(), GameplaySystemOrder.ClientPresentation + 4);
            systems.Add(new ClientSettlementInteractionPromptStateSystem(), GameplaySystemOrder.ClientPresentation + 5);
            systems.Add(new ClientSettlementTransferFeedbackSystem(), GameplaySystemOrder.ClientPresentation + 6);
            systems.Add(new ClientSettlementHudIntentSystem(), GameplaySystemOrder.ClientPresentation + 7);
            systems.Add(new StateDrivenEcsWindowHostSystem<ClientCoreWT, SettlementHudWindow, SettlementHudSession>(
                static (in SettlementHudSession state) => state.IsVisible), GameplaySystemOrder.ClientPresentation + 8);
            systems.Add(hudBridge, GameplaySystemOrder.ClientPresentation + 9);
            systems.Add(new StateDrivenEcsWindowHostSystem<ClientCoreWT, InteractionPromptWindow, InteractionPromptState>(
                static (in InteractionPromptState state) => state.IsVisible), GameplaySystemOrder.ClientPresentation + 10);
            systems.Add(promptBridge, GameplaySystemOrder.ClientPresentation + 11);
            systems.Add(new StateDrivenEcsWindowHostSystem<ClientCoreWT, BuildingManagementPanelWindow, BuildingPanelSession>(
                static (in BuildingPanelSession state) => state.IsOpen), GameplaySystemOrder.ClientPresentation + 12);
            systems.Add(buildingPanelBridge, GameplaySystemOrder.ClientPresentation + 13);

            systems.Add(new ClientSettlementContextPanelSessionSystem(), GameplaySystemOrder.ClientPresentation + 14);
            systems.Add(new StateDrivenEcsWindowHostSystem<ClientCoreWT, SettlementContextPanelWindow, SettlementContextPanelSession>(
                static (in SettlementContextPanelSession state) => state.Mode != SettlementContextPanelMode.None), GameplaySystemOrder.ClientPresentation + 15);
            systems.Add(contextPanelBridge, GameplaySystemOrder.ClientPresentation + 16);
        }
    }
}
