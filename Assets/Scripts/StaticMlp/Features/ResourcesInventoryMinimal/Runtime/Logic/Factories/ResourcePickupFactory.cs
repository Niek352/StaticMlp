using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Game;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public sealed class ResourcePickupFactory : NetEntityFactory<ResourcePickupNetworkEntity>, IResource
    {
        public EntityGID Spawn(
            ResourceAmount resource,
            Vector3 position,
            EntityGID sourcePlayer,
            long placementId)
        {
            var entity = CreateEntity(
                new NetworkPeerId(0),
                NetworkAuthority.Server,
                ResourcesInventoryMinimalGameplayFeature.RESOURCE_PICKUP);
            Configure(entity, resource, position, sourcePlayer, placementId);
            SendEntity(entity);
            return entity.GID;
        }

        private static void Configure(
            SW.Entity entity,
            ResourceAmount resource,
            Vector3 position,
            EntityGID sourcePlayer,
            long placementId)
        {
            entity.Set<ResourcePickupTag>();
            entity.Set(new ResourcePickup
            {
                ResourceId = resource.Id.Value,
                Amount = resource.Amount,
                Position = position
            });
            entity.Set(new ResourcePickupSource
            {
                SourcePlayer = sourcePlayer,
                PlacementId = placementId
            });
        }
    }
}
