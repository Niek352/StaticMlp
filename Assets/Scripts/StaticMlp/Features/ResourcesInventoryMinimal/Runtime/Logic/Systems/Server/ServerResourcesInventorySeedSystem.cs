using FFS.Libraries.StaticEcs;
using StaticMlp.Game.Components;
using StaticMlp.Networking;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public sealed class ServerResourcesInventorySeedSystem : ISystem
    {
        public void Update()
        {
            foreach (var e in SW.Query<All<PlayerTag>, None<ResourcesInventory>>().Entities())
                ResourcesInventoryAccess.Initialize(e, ResourcesInventory.MAX_SLOTS);
        }
    }
}
