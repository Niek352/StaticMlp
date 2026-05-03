using FFS.Libraries.StaticEcs;
using StaticMlp.Features.BuildingCatalog;
using StaticMlp.Game.Components.Buildings;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.Buildings
{
    public static class ServerBuildingSpawns
    {
        private const ushort NetworkedEntityCluster = 1;

        public static EntityGID SpawnConstructionSite(
            NetworkPeerId owner,
            in BuildingDefinition definition,
            Vector3 position,
            Quaternion rotation)
        {
            EnsureNetworkedEntityCluster();
            var e = SW.NewEntity<Default>(NetworkedEntityCluster);

            e.Set(new NetworkIdentity
            {
                Owner = owner,
                Authority = NetworkAuthority.Server,
                NetworkArchetypeId = definition.BlueprintArchetypeId
            });

            e.Set<NetworkedTag>();
            e.Set<ConstructionSiteTag>();
            e.Set(new ConstructionSiteState
            {
                BuildingId = definition.Id.Value,
                Phase = ConstructionPhase.WaitingForResources
            });
            e.Set(new ConstructionTransform
            {
                Position = position,
                Rotation = rotation
            });
            e.Set(new ConstructionResources
            {
                WoodRequired = definition.CostWood,
                StoneRequired = definition.CostStone
            });
            e.Set(new ConstructionProgress
            {
                BuildWorkRequired = definition.BuildWorkRequired
            });
            e.Set(new BuildingFootprint(definition.FootprintWidth, definition.FootprintLength));

            OwnershipTags.ApplyForServer(e, owner, NetworkAuthority.Server);
            SpawnBroadcaster.SendSpawn(e);
            return e.GID;
        }

        public static EntityGID SpawnFinishedBuilding(
            NetworkPeerId owner,
            in BuildingDefinition definition,
            in ConstructionTransform transform)
        {
            EnsureNetworkedEntityCluster();
            var e = SW.NewEntity<Default>(NetworkedEntityCluster);

            e.Set(new NetworkIdentity
            {
                Owner = owner,
                Authority = NetworkAuthority.Server,
                NetworkArchetypeId = definition.FinishedArchetypeId
            });

            e.Set<NetworkedTag>();
            e.Set<FinishedBuildingTag>();
            e.Set(new ConstructionSiteState
            {
                BuildingId = definition.Id.Value,
                Phase = ConstructionPhase.Completed
            });
            e.Set(transform);
            e.Set(new ConstructionResources
            {
                WoodRequired = definition.CostWood,
                StoneRequired = definition.CostStone,
                WoodDelivered = definition.CostWood,
                StoneDelivered = definition.CostStone
            });
            e.Set(new ConstructionProgress
            {
                BuildWorkRequired = definition.BuildWorkRequired,
                BuildWorkDone = definition.BuildWorkRequired
            });
            e.Set(new BuildingFootprint(definition.FootprintWidth, definition.FootprintLength));

            OwnershipTags.ApplyForServer(e, owner, NetworkAuthority.Server);
            SpawnBroadcaster.SendSpawn(e);
            return e.GID;
        }

        private static void EnsureNetworkedEntityCluster()
        {
            if (!SW.ClusterIsRegistered(NetworkedEntityCluster))
                SW.RegisterCluster(NetworkedEntityCluster);
        }
    }
}
