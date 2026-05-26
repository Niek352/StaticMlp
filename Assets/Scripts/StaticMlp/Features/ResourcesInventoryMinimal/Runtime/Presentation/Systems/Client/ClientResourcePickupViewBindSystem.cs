using FFS.Libraries.StaticEcs;
using StaticMlp.Features.EcsViews;
using StaticMlp.Game.Presentation;
using StaticMlp.Networking;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    /// <summary>
    /// Runs before <see cref="BindEntityViewSystem"/> (order 310).
    /// Sets <see cref="ViewTransform.RenderPosition"/> from <see cref="ResourcePickup.Position"/>
    /// the moment the archetype components are added, so the first Apply never places the view at origin.
    /// </summary>
    public sealed class ClientResourcePickupViewBindSystem : ISystem
    {
        public void Update()
        {
            foreach (var entity in CW.Query<All<ResourcePickup, ViewTransform>, AllAdded<ResourcePickup>>().Entities())
            {
                ref readonly var pickup = ref entity.Read<ResourcePickup>();
                ref var viewTransform = ref entity.Mut<ViewTransform>();
                viewTransform.RenderPosition = pickup.Position;
            }
        }
    }
}
