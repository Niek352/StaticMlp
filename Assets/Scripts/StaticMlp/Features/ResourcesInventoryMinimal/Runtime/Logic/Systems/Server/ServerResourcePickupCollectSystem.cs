using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Ownership;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public sealed class ServerResourcePickupCollectSystem : ISystem
    {
        public void Update()
        {
            var config = SW.GetResource<ResourcesInventoryConfig>();
            var radiusSq = config.PickupCollectionRadius * config.PickupCollectionRadius;

            foreach (var pickup in SW.Query<All<ServerOwned, ResourcePickupTag, ResourcePickup, ResourcePickupSource>>().Entities())
            {
                ref readonly var pickupState = ref pickup.Read<ResourcePickup>();
                pickupState.Validate();

                ref readonly var source = ref pickup.Read<ResourcePickupSource>();
                if (!TryFindCollector(source.SourcePlayer, pickupState.Position, radiusSq, out var collector))
                    continue;

                var rejected = ResourcesInventoryAccess.Add(
                    collector,
                    new ResourceAmount(new ResourceId(pickupState.ResourceId), pickupState.Amount));
                if (rejected == pickupState.Amount)
                    continue;

                if (rejected > 0)
                {
                    ref var mutablePickup = ref ReplicationMut.Mut<ResourcePickup>(pickup);
                    mutablePickup.Amount = rejected;
                    mutablePickup.Validate();
                    continue;
                }

                NetworkEntityDespawner.DespawnAndDestroy(pickup);
            }
        }

        private static bool TryFindCollector(
            EntityGID sourcePlayer,
            Vector3 pickupPosition,
            float radiusSq,
            out SW.Entity collector)
        {
            if (TryGetEligiblePlayer(sourcePlayer, out var source)
                && IsWithinRadius(source.Read<CharacterNetState>().Position, pickupPosition, radiusSq))
            {
                collector = source;
                return true;
            }

            var bestDistanceSq = float.MaxValue;
            collector = default;
            var found = false;
            foreach (var player in SW.Query<All<PlayerTag, CharacterNetState, ResourcesInventory>>().Entities())
            {
                var distanceSq = (player.Read<CharacterNetState>().Position - pickupPosition).sqrMagnitude;
                if (distanceSq > radiusSq || distanceSq >= bestDistanceSq)
                    continue;

                bestDistanceSq = distanceSq;
                collector = player;
                found = true;
            }

            return found;
        }

        private static bool TryGetEligiblePlayer(EntityGID playerGid, out SW.Entity player)
        {
            if (!playerGid.TryUnpack<ServerWT>(out player))
                return false;

            return player.Has<PlayerTag>()
                   && player.Has<CharacterNetState>()
                   && player.Has<ResourcesInventory>();
        }

        private static bool IsWithinRadius(Vector3 playerPosition, Vector3 pickupPosition, float radiusSq) =>
            (playerPosition - pickupPosition).sqrMagnitude <= radiusSq;
    }
}
