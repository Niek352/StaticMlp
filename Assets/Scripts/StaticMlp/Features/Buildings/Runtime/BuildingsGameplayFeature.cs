using System;
using Aspid.StaticEcs;
using Aspid.StaticEcs.Windows;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.EcsViews;
using StaticMlp.Features.Interaction;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;
using StaticMlp.Networking.Requests;
using StaticMlp.Networking.Replication;
using UnityEngine;
using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.Buildings
{
    public sealed class BuildingsGameplayFeature : GameplayFeature
    {
        private const string BUILDING_MENU_VIEW_RESOURCE_PATH = "Views/Buildings/BuildingMenu";

        public override void RegisterNetworkEvents()
        {
            ProjectionRegistry.Register<ConstructionResources>();
            ProjectionRegistry.RegisterMulti<ConstructionResourceEntry>();
            ProjectionRegistry.Register<ConstructionSiteState>();
            ProjectionRegistry.Register<ConstructionProgress>();
            ProjectionRegistry.Register<SettlementAnchorRef>();
            DepositConstructionResourcesEventCodec.Register();
            RequestRegistry.Register<BuildConstructionRequestEvent, BuildConstructionResultEvent>(
                new BuildConstructionHandler(),
                new BuildConstructionProjector(), GameplaySystemOrder.Gameplay - 60);
            RequestRegistry.Register<DepositConstructionResourcesRequestEvent, DepositConstructionResourcesResultEvent>(
                new DepositConstructionResourcesHandler(),
                new DepositConstructionResourcesProjector(),
                GameplaySystemOrder.Gameplay - 70);
        }

        public override void RegisterPrefabs()
        {
            for (var i = 0; i < BuildingCatalogData.All.Count; i++)
            {
                var definition = BuildingCatalogData.All[i];
                RegisterBuilding(definition);
            }
        }

        public override void RegisterServerResources()
        {
            SW.SetResource(new BuildingEntityFactory());
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerInitialConstructionSiteSpawnSystem(), (short)(GameplaySystemOrder.ServerConnectionGameplay - 10));
            systems.Add(new ServerPlaceBuildingRequestSystem(), GameplaySystemOrder.Gameplay - 80);
            systems.Add(new ServerCompleteConstructionSystem(), GameplaySystemOrder.Gameplay - 50);
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            var windows = CW.GetResource<WindowsController<ClientCoreWT>>();
            var registry = CW.GetResource<EcsLinkRegistry<ClientCoreWT>>();
            registry.RegisterComponent<BuildingMenuViewModel, BuildingMenuViewData>(
                Binding);

            windows.RegisterWindow<BuildingMenuWindow, EcsWindowNoData, BuildingMenuShellView>(
                EcsResourcesWindowShellViewFactory.CreateLazy<BuildingMenuShellView>(BUILDING_MENU_VIEW_RESOURCE_PATH),
                EcsWindowLayer.Persistent,
                persistentSortOrder: 80);
            windows.RegisterLinkedViewModel<BuildingMenuWindow, EcsWindowNoData, BuildingMenuSlot, BuildingMenuViewModel>(
                static _ => new BuildingMenuViewModel(),
                static _ => ResolveSingletonPresentationEntity<BuildingMenuViewData>(),
                EcsWindowOpenInputBindings.Ignore<ClientCoreWT, BuildingMenuWindow, EcsWindowNoData, BuildingMenuViewModel>);

            systems.Add(new ClientConstructionInteractableFocusPointSystem(), (short)(GameplaySystemOrder.ClientInput + 10));
            systems.Add(new ClientBuildingMenuSystem(), (short)(GameplaySystemOrder.ClientInput + 20));
            systems.Add(new ClientBuildingMenuIntentSystem(), (short)(GameplaySystemOrder.ClientInput + 21));
            systems.Add(new ClientPlacementInputSystem(), (short)(GameplaySystemOrder.Gameplay - 90));
            systems.Add(new ClientPlacementValidationPreviewSystem(), (short)(GameplaySystemOrder.Gameplay - 85));
            systems.Add(new ClientPlacementConfirmSystem(), (short)(GameplaySystemOrder.Gameplay - 80));
            systems.Add(new ClientConstructionInteractionSystem(), (short)(GameplaySystemOrder.Gameplay - 5));
            systems.Add(new ClientConstructionViewStateSystem(), ViewSystemOrder.BuildPresentationState);
            systems.Add(new ClientBuildingMenuPresentationBootstrapSystem(), (short)(GameplaySystemOrder.ClientPresentation + 19));
            systems.Add(new StateDrivenEcsWindowHostSystem<ClientCoreWT, BuildingMenuWindow, BuildingMenuState>(
                static (in BuildingMenuState state) => state.IsOpen), (short)(GameplaySystemOrder.ClientPresentation + 20));
            systems.Add(new ClientBuildingMenuBridgeSystem(), (short)(GameplaySystemOrder.ClientPresentation + 21));
        }

        private static void Binding(BuildingMenuViewModel viewModel, in BuildingMenuViewData data)
        {
            viewModel.Apply(in data);
        }

        public override void RegisterClientViewSync(ViewSyncBuilder views)
        {
            views.Register<PlacementPreviewViewState>();
            views.Register<ConstructionViewState>();
        }

        private static EntityGID ResolveSingletonPresentationEntity<TComponent>()
            where TComponent : struct, IComponent
        {
            foreach (var entity in CW.Query<All<TComponent>>().Entities())
                return entity.GID;

            throw new InvalidOperationException($"{typeof(TComponent).FullName} entity is missing.");
        }

        private static void RegisterBuilding(BuildingDefinition definition)
        {
            if (!BuildingNetworkCatalog.TryGet(definition.Id, out var network))
                throw new InvalidOperationException($"Missing network catalog entry for building {definition.Id}.");

            if (!BuildingPresentationCatalog.TryGet(definition.Id, out var presentation))
                throw new InvalidOperationException($"Missing presentation catalog entry for building {definition.Id}.");

            NetArchetypeRegistry.RegisterClient(network.BlueprintArchetypeId, e =>
            {
                e.Set<ConstructionSiteTag>();
                e.Set<InteractableTag>();
                e.Set(new Interactable { Kind = InteractableKind.ConstructionSite });
                e.Set(new InteractableFocusPoint { Radius = ResolveInteractionRadius(definition) });
                e.Set(new BuildingFootprint(definition.FootprintWidth, definition.FootprintLength));
                e.Set(new ViewTransform
                {
                    RenderRotation = Quaternion.identity
                });
                e.Set(new ConstructionViewState());
                e.Set(new ViewPath(presentation.BlueprintViewPath));
            });

            NetArchetypeRegistry.RegisterServer(network.BlueprintArchetypeId, e =>
            {
                e.Set<ConstructionSiteTag>();
                e.Set(new BuildingFootprint(definition.FootprintWidth, definition.FootprintLength));
            });

            NetArchetypeRegistry.RegisterClient(network.FinishedArchetypeId, e =>
            {
                e.Set<FinishedBuildingTag>();
                e.Set<InteractableTag>();
                e.Set(new Interactable { Kind = InteractableKind.FinishedBuilding });
                e.Set(new InteractableFocusPoint { Radius = ResolveInteractionRadius(definition) });
                e.Set(new BuildingFootprint(definition.FootprintWidth, definition.FootprintLength));
                e.Set(new ViewTransform
                {
                    RenderRotation = Quaternion.identity
                });
                e.Set(CreateCompletedConstructionViewState(definition.ConstructionCost));
                e.Set(new ViewPath(presentation.FinishedViewPath));
            });

            NetArchetypeRegistry.RegisterServer(network.FinishedArchetypeId, e =>
            {
                e.Set<FinishedBuildingTag>();
                e.Set(new BuildingFootprint(definition.FootprintWidth, definition.FootprintLength));
            });
        }

        private static ConstructionViewState CreateCompletedConstructionViewState(ResourceAmount[] constructionCost)
        {
            var state = new ConstructionViewState
            {
                Phase = ConstructionPhase.Completed,
                Progress01 = 1f
            };

            for (var i = 0; i < constructionCost.Length; i++)
            {
                if (state.Resources.Length == state.Resources.Capacity)
                    throw new InvalidOperationException(
                        $"{nameof(ConstructionViewState)} cannot hold more than {state.Resources.Capacity} resource rows.");

                var cost = constructionCost[i];
                state.Resources.Add(new ConstructionResourceViewEntry(cost.Id, cost.Amount, cost.Amount));
            }

            return state;
        }

        private static float ResolveInteractionRadius(BuildingDefinition definition)
        {
            return Mathf.Max(1.25f, Mathf.Sqrt(
                definition.FootprintWidth * definition.FootprintWidth
                + definition.FootprintLength * definition.FootprintLength) * 0.5f);
        }
    }
}
