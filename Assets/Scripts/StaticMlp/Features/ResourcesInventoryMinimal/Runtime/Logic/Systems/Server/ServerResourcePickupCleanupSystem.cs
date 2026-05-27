using FFS.Libraries.StaticEcs;
using StaticMlp.Game;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    /// <summary>
    /// Despawns collected pickup entities when their server tick deadline expires,
    /// giving the client enough time to play the magnet animation.
    /// </summary>
    public sealed class ServerResourcePickupCleanupSystem : ISystem
    {
        public void Update()
        {
            var currentTick = SW.GetResource<SimulationTime>().ServerTick;
            foreach (var entity in SW.Query<All<ResourcePickupDespawnTimer>>().Entities())
            {
                ref readonly var timer = ref entity.Read<ResourcePickupDespawnTimer>();
                if (currentTick < timer.DespawnAtTick)
                    continue;

                NetworkEntityDespawner.DespawnAndDestroy(entity);
            }
        }
    }
}
