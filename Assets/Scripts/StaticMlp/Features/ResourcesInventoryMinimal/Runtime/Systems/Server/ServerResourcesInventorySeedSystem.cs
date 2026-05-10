using FFS.Libraries.StaticEcs;
using StaticMlp.Features.Settlement;
using StaticMlp.Game.Components;
using StaticMlp.Networking;
using StaticMlp.Networking.Replication;

namespace StaticMlp.Features.ResourcesInventoryMinimal
{
    public sealed class ServerResourcesInventorySeedSystem : ISystem
    {
        public void Update()
        {
            var settlementSeed = SW.GetResource<Stage1SettlementSeed>();
            var startingWood = settlementSeed.GetStartingResourceAmount(ResourceCatalog.WoodId);
            var startingStone = settlementSeed.GetStartingResourceAmount(ResourceCatalog.StoneId);

            foreach (var e in SW.Query<All<PlayerTag>, None<ResourcesInventory>>().Entities())
            {
                e.Set(new ResourcesInventory
                {
                    Wood = startingWood,
                    Stone = startingStone
                });
                ReplicationMut.MarkDataDirty(e);
            }
        }
    }
}
