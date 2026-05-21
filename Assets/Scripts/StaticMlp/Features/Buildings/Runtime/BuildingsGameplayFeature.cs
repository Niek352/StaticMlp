using System;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.EcsViews;
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
            systems.Add(new ClientBuildingMenuSystem(), (short)(GameplaySystemOrder.ClientInput + 20));
            systems.Add(new ClientPlacementInputSystem(), (short)(GameplaySystemOrder.Gameplay - 90));
            systems.Add(new ClientPlacementValidationPreviewSystem(), (short)(GameplaySystemOrder.Gameplay - 85));
            systems.Add(new ClientPlacementConfirmSystem(), (short)(GameplaySystemOrder.Gameplay - 80));
            systems.Add(new ClientConstructionInteractionSystem(), (short)(GameplaySystemOrder.Gameplay - 5));
            systems.Add(new ClientConstructionViewStateSystem(), ViewSystemOrder.BuildPresentationState);
            systems.Add(new ClientBuildingMenuMvcSystem(), (short)(GameplaySystemOrder.ClientPresentation + 20));
        }

        public override void RegisterClientViewSync(ViewSyncBuilder views)
        {
            views.Register<PlacementPreviewViewState>();
            views.Register<ConstructionViewState>();
        }

        private static void RegisterBuilding(BuildingDefinition definition)
        {
            if (!BuildingNetworkCatalog.TryGet(definition.Id, out var network))
                throw new InvalidOperationException($"Missing network catalog entry for building {definition.Id}.");

            if (!BuildingPresentationCatalog.TryGet(definition.Id, out var presentation))
                throw new InvalidOperationException($"Missing presentation catalog entry for building {definition.Id}.");

            var woodCost = definition.GetConstructionCost(ResourceCatalog.WoodId);
            var stoneCost = definition.GetConstructionCost(ResourceCatalog.StoneId);

            NetArchetypeRegistry.RegisterClient(network.BlueprintArchetypeId, e =>
            {
                e.Set<ConstructionSiteTag>();
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
                e.Set(new BuildingFootprint(definition.FootprintWidth, definition.FootprintLength));
                e.Set(new ViewTransform
                {
                    RenderRotation = Quaternion.identity
                });
                e.Set(new ConstructionViewState
                {
                    Phase = ConstructionPhase.Completed,
                    WoodRequired = woodCost,
                    StoneRequired = stoneCost,
                    WoodDelivered = woodCost,
                    StoneDelivered = stoneCost,
                    Progress01 = 1f
                });
                e.Set(new ViewPath(presentation.FinishedViewPath));
            });

            NetArchetypeRegistry.RegisterServer(network.FinishedArchetypeId, e =>
            {
                e.Set<FinishedBuildingTag>();
                e.Set(new BuildingFootprint(definition.FootprintWidth, definition.FootprintLength));
            });
        }
    }
}
