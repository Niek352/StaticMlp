using FFS.Libraries.StaticEcs;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;
using UnityEngine;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    /// <summary>
    /// Counts down <see cref="ResourcePickupDespawnTimer"/> and despawns the entity
    /// once the timer expires, giving the client enough time to play the magnet animation.
    /// </summary>
    public sealed class ServerResourcePickupCleanupSystem : ISystem
    {
        public void Update()
        {
            foreach (var entity in SW.Query<All<ResourcePickupDespawnTimer>>().Entities())
            {
                ref var timer = ref entity.Mut<ResourcePickupDespawnTimer>();
                timer.RemainingSeconds -= Time.deltaTime;

                if (timer.RemainingSeconds <= 0f)
                    NetworkEntityDespawner.DespawnAndDestroy(entity);
            }
        }
    }
}
