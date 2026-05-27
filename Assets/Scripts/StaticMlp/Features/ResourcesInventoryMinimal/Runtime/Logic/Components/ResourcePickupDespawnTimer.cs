using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    /// <summary>
    /// Server-only (non-replicated) component. Tracks the server tick when a collected
    /// pickup entity is despawned, giving clients time to play the magnet animation.
    /// </summary>
    public struct ResourcePickupDespawnTimer : IComponent
    {
        public uint DespawnAtTick;
    }
}
