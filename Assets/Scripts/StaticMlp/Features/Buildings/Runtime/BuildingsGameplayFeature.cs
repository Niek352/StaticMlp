using System;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Bootstrap;
using StaticMlp.Game.Components.Buildings;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    public sealed class BuildingsGameplayFeature : GameplayFeature
    {
        public override void RegisterNetworkEvents()
        {
            ConstructionEventCodec.Register();
        }

        public override void RegisterPrefabs()
        {
            for (var i = 0; i < StaticMlp.Features.BuildingCatalog.BuildingCatalog.All.Count; i++)
            {
                var definition = StaticMlp.Features.BuildingCatalog.BuildingCatalog.All[i];
                RegisterBuilding(definition);
            }
        }

        public override void RegisterServerSystems(ServerSystemsBuilder systems)
        {
            systems.Add(new ServerPlaceBuildingRequestSystem(), GameplaySystemOrder.Gameplay - 80);
            systems.Add(new ServerDepositConstructionResourcesSystem(), GameplaySystemOrder.Gameplay - 70);
            systems.Add(new ServerBuildConstructionSystem(), GameplaySystemOrder.Gameplay - 60);
            systems.Add(new ServerCompleteConstructionSystem(), GameplaySystemOrder.Gameplay - 50);
        }

        public override void RegisterClientCoreSystems(ClientCoreSystemsBuilder systems)
        {
            systems.Add(new ClientBuildingMenuSystem(), GameplaySystemOrder.Gameplay - 90);
            systems.Add(new ClientPlacementInputSystem(), GameplaySystemOrder.Gameplay - 80);
            systems.Add(new ClientPlacementValidationPreviewSystem(), GameplaySystemOrder.Gameplay - 70);
            systems.Add(new ClientPlacementConfirmSystem(), GameplaySystemOrder.Gameplay - 60);
            systems.Add(new ClientConstructionInteractionSystem(), GameplaySystemOrder.Gameplay - 50);
            systems.Add(new ClientConstructionViewStateSystem(), ViewSystemOrder.BuildPresentationState);
        }

        public override void RegisterClientViewSync(ViewSyncBuilder views)
        {
            views.Register<PlacementPreviewViewState>();
            views.Register<ConstructionViewState>();
        }

        private static void RegisterBuilding(BuildingDefinition definition)
        {
            if (!BuildingNetworkCatalog.TryGetDefinition(definition.Id, out var network))
                throw new InvalidOperationException($"Missing network catalog entry for building {definition.Id}.");

            if (!BuildingPresentationCatalog.TryGetDefinition(definition.Id, out var presentation))
                throw new InvalidOperationException($"Missing presentation catalog entry for building {definition.Id}.");

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
                    WoodRequired = definition.CostWood,
                    StoneRequired = definition.CostStone,
                    WoodDelivered = definition.CostWood,
                    StoneDelivered = definition.CostStone,
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
