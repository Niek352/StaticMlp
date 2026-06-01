using Aspid.StaticEcs;
using Aspid.StaticEcs.Windows;
using FFS.Libraries.StaticEcs;
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
            var registry = CW.GetResource<EcsLinkRegistry<ClientCoreWT>>();
            registry.RegisterComponent<SettlementHudViewModel, SettlementHudViewData>(
                Binding);
            registry.RegisterComponent<InteractionPromptViewModel, InteractionPromptViewData>(
                EcsComponentBinding);
            registry.RegisterComponent<BuildingManagementPanelViewModel, BuildingManagementPanelViewData>(
                ComponentBinding);
            registry.RegisterComponent<SettlementContextPanelViewModel, SettlementContextPanelViewData>(
                Binding1);

            windows.RegisterWindow<SettlementHudWindow, EcsWindowNoData, SettlementHudShellView>(
                EcsResourcesWindowShellViewFactory.CreateLazy<SettlementHudShellView>(HUD_VIEW_RESOURCE_PATH),
                EcsWindowLayer.Persistent,
                persistentSortOrder: 0);
            windows.RegisterLinkedViewModel<SettlementHudWindow, EcsWindowNoData, SettlementHudSlot, SettlementHudViewModel>(
                static _ => new SettlementHudViewModel(),
                static _ => ResolveSingletonPresentationEntity<SettlementHudViewData>(),
                EcsWindowOpenInputBindings.Ignore<ClientCoreWT, SettlementHudWindow, EcsWindowNoData, SettlementHudViewModel>);

            windows.RegisterWindow<InteractionPromptWindow, EcsWindowNoData, InteractionPromptShellView>(
                EcsResourcesWindowShellViewFactory.CreateLazy<InteractionPromptShellView>(INTERACTION_PROMPT_VIEW_RESOURCE_PATH),
                EcsWindowLayer.Persistent,
                persistentSortOrder: 95);
            windows.RegisterLinkedViewModel<InteractionPromptWindow, EcsWindowNoData, InteractionPromptSlot, InteractionPromptViewModel>(
                static _ => new InteractionPromptViewModel(),
                static _ => ResolveSingletonPresentationEntity<InteractionPromptViewData>(),
                EcsWindowOpenInputBindings.Ignore<ClientCoreWT, InteractionPromptWindow, EcsWindowNoData, InteractionPromptViewModel>);

            windows.RegisterWindow<BuildingManagementPanelWindow, EcsWindowNoData, BuildingManagementPanelShellView>(
                EcsResourcesWindowShellViewFactory.CreateLazy<BuildingManagementPanelShellView>(BUILDING_MANAGEMENT_PANEL_VIEW_RESOURCE_PATH),
                EcsWindowLayer.Persistent,
                persistentSortOrder: 110);
            windows.RegisterLinkedViewModel<BuildingManagementPanelWindow, EcsWindowNoData, BuildingManagementPanelSlot, BuildingManagementPanelViewModel>(
                static _ => new BuildingManagementPanelViewModel(),
                static _ => ResolveSingletonPresentationEntity<BuildingManagementPanelViewData>(),
                EcsWindowOpenInputBindings.Ignore<ClientCoreWT, BuildingManagementPanelWindow, EcsWindowNoData, BuildingManagementPanelViewModel>);

            windows.RegisterWindow<SettlementContextPanelWindow, EcsWindowNoData, SettlementContextPanelShellView>(
                EcsResourcesWindowShellViewFactory.CreateLazy<SettlementContextPanelShellView>(CONTEXT_PANEL_VIEW_RESOURCE_PATH),
                EcsWindowLayer.Persistent,
                persistentSortOrder: 100);
            windows.RegisterLinkedViewModel<SettlementContextPanelWindow, EcsWindowNoData, SettlementContextPanelSlot, SettlementContextPanelViewModel>(
                static _ => new SettlementContextPanelViewModel(),
                static _ => ResolveSingletonPresentationEntity<SettlementContextPanelViewData>(),
                EcsWindowOpenInputBindings.Ignore<ClientCoreWT, SettlementContextPanelWindow, EcsWindowNoData, SettlementContextPanelViewModel>);

            systems.Add(new ClientSettlementPresentationBootstrapSystem(), GameplaySystemOrder.ClientPresentation + 1);
            systems.Add(new ClientBuildingManagementPanelInteractionOpenSystem(), GameplaySystemOrder.ClientPresentation + 2);
            systems.Add(new ClientBuildingManagementPanelCloseInputSystem(), GameplaySystemOrder.ClientPresentation + 3);
            systems.Add(new ClientBuildingManagementPanelActionIntentSystem(), GameplaySystemOrder.ClientPresentation + 4);
            systems.Add(new ClientBuildingManagementPanelLifetimeSystem(), GameplaySystemOrder.ClientPresentation + 5);
            systems.Add(new ClientSettlementInteractionPromptStateSystem(), GameplaySystemOrder.ClientPresentation + 6);
            systems.Add(new ClientSettlementTransferFeedbackSystem(), GameplaySystemOrder.ClientPresentation + 7);
            systems.Add(new ClientSettlementHudIntentSystem(), GameplaySystemOrder.ClientPresentation + 8);
            systems.Add(new StateDrivenEcsWindowHostSystem<ClientCoreWT, SettlementHudWindow, SettlementHudSession>(
                static (in SettlementHudSession state) => state.IsVisible), GameplaySystemOrder.ClientPresentation + 9);
            systems.Add(hudBridge, GameplaySystemOrder.ClientPresentation + 10);
            systems.Add(new StateDrivenEcsWindowHostSystem<ClientCoreWT, InteractionPromptWindow, InteractionPromptState>(
                static (in InteractionPromptState state) => state.IsVisible), GameplaySystemOrder.ClientPresentation + 11);
            systems.Add(promptBridge, GameplaySystemOrder.ClientPresentation + 12);
            systems.Add(new StateDrivenEcsWindowHostSystem<ClientCoreWT, BuildingManagementPanelWindow, BuildingPanelSession>(
                static (in BuildingPanelSession state) => state.IsOpen), GameplaySystemOrder.ClientPresentation + 13);
            systems.Add(buildingPanelBridge, GameplaySystemOrder.ClientPresentation + 14);

            systems.Add(new ClientSettlementContextPanelSessionSystem(), GameplaySystemOrder.ClientPresentation + 15);
            systems.Add(new ClientSettlementContextPanelActionIntentSystem(), GameplaySystemOrder.ClientPresentation + 16);
            systems.Add(new StateDrivenEcsWindowHostSystem<ClientCoreWT, SettlementContextPanelWindow, SettlementContextPanelSession>(
                static (in SettlementContextPanelSession state) => state.Mode != SettlementContextPanelMode.None), GameplaySystemOrder.ClientPresentation + 17);
            systems.Add(contextPanelBridge, GameplaySystemOrder.ClientPresentation + 18);
        }

        private static void Binding1(SettlementContextPanelViewModel viewModel, in SettlementContextPanelViewData data)
        {
            viewModel.Apply(in data);
        }

        private static void ComponentBinding(BuildingManagementPanelViewModel viewModel, in BuildingManagementPanelViewData data)
        {
            viewModel.Apply(in data);
        }

        private static void EcsComponentBinding(InteractionPromptViewModel viewModel, in InteractionPromptViewData data)
        {
            viewModel.Apply(in data);
        }

        private static void Binding(SettlementHudViewModel viewModel, in SettlementHudViewData data)
        {
            viewModel.Apply(in data);
        }

        private static EntityGID ResolveSingletonPresentationEntity<TComponent>()
            where TComponent : struct, IComponent
        {
            foreach (var entity in CW.Query<All<TComponent>>().Entities())
                return entity.GID;

            throw new System.InvalidOperationException($"{typeof(TComponent).FullName} entity is missing.");
        }
    }
}
