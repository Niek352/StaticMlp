using FFS.Libraries.StaticEcs;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    /// <summary>
    /// Server-only (non-replicated) component. Tracks remaining seconds before a collected
    /// pickup entity is despawned, giving the client time to play the magnet animation.
    /// </summary>
    public struct ResourcePickupDespawnTimer : IComponent
    {
        public float RemainingSeconds;
    }
}
