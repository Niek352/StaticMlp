using System;
using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Game.Components.Buildings;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    public static class ServerBuildingSpawns
    {
        public static EntityGID SpawnConstructionSite(
            NetworkPeerId owner,
            in BuildingDefinition definition,
            Vector3 position,
            Quaternion rotation)
        {
            if (!BuildingNetworkCatalog.TryGetDefinition(definition.Id, out var network))
                throw new InvalidOperationException($"Missing network catalog entry for building {definition.Id}.");

            var localDefinition = definition;
            return NetworkEntitySpawner.SpawnServerEntity(
                owner,
                NetworkAuthority.Server,
                network.BlueprintArchetypeId,
                entity => InitializeConstructionSite(entity, in localDefinition, position, rotation));
        }

        public static EntityGID SpawnFinishedBuilding(
            NetworkPeerId owner,
            in BuildingDefinition definition,
            in ConstructionTransform transform)
        {
            if (!BuildingNetworkCatalog.TryGetDefinition(definition.Id, out var network))
                throw new InvalidOperationException($"Missing network catalog entry for building {definition.Id}.");

            var localDefinition = definition;
            var localTransform = transform;
            return NetworkEntitySpawner.SpawnServerEntity(
                owner,
                NetworkAuthority.Server,
                network.FinishedArchetypeId,
                entity => InitializeFinishedBuilding(entity, in localDefinition, in localTransform));
        }

        private static void InitializeConstructionSite(
            SW.Entity entity,
            in BuildingDefinition definition,
            Vector3 position,
            Quaternion rotation)
        {
            entity.Set<ConstructionSiteTag>();
            entity.Set(new ConstructionSiteState
            {
                BuildingId = definition.Id.Value,
                Phase = ConstructionPhase.WaitingForResources
            });
            entity.Set(new ConstructionTransform
            {
                Position = position,
                Rotation = rotation
            });
            entity.Set(new ConstructionResources
            {
                WoodRequired = definition.CostWood,
                StoneRequired = definition.CostStone
            });
            entity.Set(new ConstructionProgress
            {
                BuildWorkRequired = definition.BuildWorkRequired
            });
            entity.Set(new BuildingFootprint(definition.FootprintWidth, definition.FootprintLength));
        }

        private static void InitializeFinishedBuilding(
            SW.Entity entity,
            in BuildingDefinition definition,
            in ConstructionTransform transform)
        {
            entity.Set<FinishedBuildingTag>();
            entity.Set(new ConstructionSiteState
            {
                BuildingId = definition.Id.Value,
                Phase = ConstructionPhase.Completed
            });
            entity.Set(transform);
            entity.Set(new ConstructionResources
            {
                WoodRequired = definition.CostWood,
                StoneRequired = definition.CostStone,
                WoodDelivered = definition.CostWood,
                StoneDelivered = definition.CostStone
            });
            entity.Set(new ConstructionProgress
            {
                BuildWorkRequired = definition.BuildWorkRequired,
                BuildWorkDone = definition.BuildWorkRequired
            });
            entity.Set(new BuildingFootprint(definition.FootprintWidth, definition.FootprintLength));
        }
    }
}
